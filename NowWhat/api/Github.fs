module NowWhat.API.Github

open HttpFs.Client
open Hopac
open Thoth.Json.Net

open NowWhat.Config

(* -----------------------------------------------------------------------------

   GitHub data model

*)

type Issue =
    { Number: int
      Title: string
      Body: string
      State: string }

type Column =
    { Name: string
      Cards: (Issue * string) List } // string is cursor, would be nice to have a type alias for cursor

type Project =
    { Number: int
      Name: string
      Columns: Column List }

type ProjectRoot = { Projects: Project List }

(* -----------------------------------------------------------------------------
   Get Forecast objects as F# types
   *)

let issueDecoder: Decoder<Issue * string> =
    Decode.object (fun get ->
        ({ Issue.Number = get.Required.At [ "node"; "content"; "number" ] Decode.int
           Issue.Title = get.Required.At [ "node"; "content"; "title" ] Decode.string
           Issue.Body = get.Required.At [ "node"; "content"; "body" ] Decode.string
           Issue.State = get.Required.At [ "node"; "content"; "state" ] Decode.string },
         get.Required.Field "cursor" Decode.string))

let columnDecoder: Decoder<Column> =
    Decode.object (fun get ->
        { Column.Name = get.Required.At [ "node"; "name" ] Decode.string
          Column.Cards = get.Required.At [ "node"; "cards"; "edges" ] (Decode.list issueDecoder) })

let projectDecoder: Decoder<Project> =
    Decode.object (fun get ->
        { Project.Number = get.Required.At [ "node"; "number" ] Decode.int
          Project.Name = get.Required.At [ "node"; "name" ] Decode.string
          Project.Columns = get.Required.At [ "node"; "columns"; "edges" ] (Decode.list columnDecoder) })

let projectRootDecoder: Decoder<ProjectRoot> =
    Decode.object (fun get ->
        { ProjectRoot.Projects =
            get.Required.At
                [ "data"
                  "repository"
                  "projects"
                  "edges" ]
                (Decode.list projectDecoder) })

(* -----------------------------------------------------------------------------
   Interface to the GitHub API

*)

[<Literal>]
let GithubGraphQLEndpoint = "https://api.github.com/graphql"

[<Literal>]
let ProjectBoard = "Project Tracker"

// TODO: async?
/// Query Github GraphQL endpoint
/// body is json with GraphQL query element
/// https://docs.github.com/en/graphql/guides/forming-calls-with-graphql#communicating-with-graphql
let runGithubQuery (gitHubToken: string) body = async {
    let response = 
      GithubGraphQLEndpoint
      |> Request.createUrl Post
      |> Request.setHeader (Authorization $"bearer {gitHubToken}")
      |> Request.setHeader (UserAgent "NowWhat")
      |> Request.body (BodyString body)
      |> Request.responseAsString // UTF8-encoded
      |> run
    return response
}

// Format JSON query to enable correct parsing on Github's side
let formatQuery (q: string) = q.Replace("\n", "")

(* -----------------------------------------------------------------------------
   Public interface to this module
*)

let getProjectIssues (projectName: string) = async {
    // the parent function is only wrapping up the recursive call that deals with paging of the responses
    let githubToken = (getConfig ()).GithubToken 

    let rec getProjectIssues_page (projectName: string) cursor (acc: (Issue * string) List) = async {
        let queryTemplate =
            System.IO.File.ReadAllText $"{__SOURCE_DIRECTORY__}/queries/issues-by-project-graphql.json"

        // fill in placeholders into the query - project board name and cursor for paging
        let query =
            let cursorQuery =
                match cursor with
                | None ->
                    // Get first batch
                    queryTemplate.Replace("CURSOR", "null")
                | Some crs ->
                    // Get subsequent batches
                    queryTemplate.Replace("CURSOR", $"\\\"{crs}\\\"")

            cursorQuery.Replace("PROJECTNAME", $"\\\"{projectName}\\\"")
            |> formatQuery

        let! result = runGithubQuery githubToken query

        let issues: ProjectRoot =
            match result |> Decode.fromString projectRootDecoder with
            | Ok issues -> issues
            | Error _ -> failwith "Failed to decode"

        let issueData: (Issue * string) List =
            issues.Projects.Head.Columns
            |> List.collect (fun column -> column.Cards) 

        // Cursor points to the last item returned, used for paging of the requests
        let nextCursor =
            if issueData.Length = 0 then
                None
            else
                issueData 
                |> List.last 
                |> (fun (_, c) -> Some c)

        match nextCursor with
        | Some _ -> 
            return! (getProjectIssues_page projectName nextCursor (List.append acc issueData))
        | None -> 
            return (List.append acc issueData)

    }

    let! allIssues = getProjectIssues_page projectName None []      
    return allIssues |> List.map fst
}