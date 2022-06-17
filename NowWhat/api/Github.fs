module NowWhat.API.Github

open HttpFs.Client
open Hopac
open Thoth.Json.Net

open NowWhat.Config

(* -----------------------------------------------------------------------------

   GitHub data model

*)

type Issue =
    { number: int
      title: string
      body: string
      state: string }

type Column =
    { name: string
      cards: (Issue * string) List } // string is cursor, would be nice to have a type alias for cursor

type Project =
    { number: int
      name: string
      columns: Column List }

type ProjectRoot = { projects: Project List }

(* -----------------------------------------------------------------------------
   Get Forecast objects as F# types
   *)

let issueDecoder: Decoder<Issue * string> =
    Decode.object (fun get ->
        ({ Issue.number = get.Required.At [ "node"; "content"; "number" ] Decode.int
           Issue.title = get.Required.At [ "node"; "content"; "title" ] Decode.string
           Issue.body = get.Required.At [ "node"; "content"; "body" ] Decode.string
           Issue.state = get.Required.At [ "node"; "content"; "state" ] Decode.string },
         get.Required.Field "cursor" Decode.string))

let columnDecoder: Decoder<Column> =
    Decode.object (fun get ->
        { Column.name = get.Required.At [ "node"; "name" ] Decode.string
          Column.cards = get.Required.At [ "node"; "cards"; "edges" ] (Decode.list issueDecoder) })

let projectDecoder: Decoder<Project> =
    Decode.object (fun get ->
        { Project.number = get.Required.At [ "node"; "number" ] Decode.int
          Project.name = get.Required.At [ "node"; "name" ] Decode.string
          Project.columns = get.Required.At [ "node"; "columns"; "edges" ] (Decode.list columnDecoder) })

let projectRootDecoder: Decoder<ProjectRoot> =
    Decode.object (fun get ->
        { ProjectRoot.projects =
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
let runGithubQuery (gitHubToken: string) body =
    GithubGraphQLEndpoint
    |> Request.createUrl Post
    |> Request.setHeader (Authorization $"bearer {gitHubToken}")
    |> Request.setHeader (UserAgent "NowWhat")
    |> Request.body (BodyString body)
    |> Request.responseAsString // UTF8-encoded
    |> run

// Format JSON query to enable correct parsing on Github's side
let formatQuery (q: string) = q.Replace("\n", "")

(* -----------------------------------------------------------------------------
   Public interface to this module
*)

let getProjectIssues (projectName: string) : Issue List =
    // the parent function is only wrapping up the recursive call that deals with paging of the responses
    let githubToken = (getConfig ()).githubToken 

    let rec getProjectIssues_page (projectName: string) cursor (acc: (Issue * string) List) =
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

        let result = runGithubQuery githubToken query

        let issues: ProjectRoot =
            match result |> Decode.fromString projectRootDecoder with
            | Ok issues -> issues
            | Error _ -> failwith "Failed to decode"

        let issueData: (Issue * string) List =
            List.map (fun column -> column.cards) issues.projects.Head.columns
            |> List.concat

        // Cursor points to the last item returned, used for paging of the requests
        let nextCursor =
            if issueData.Length = 0 then
                None
            else
                issueData |> List.last |> (fun (_, c) -> Some c)

        match nextCursor with
        | Some _ -> getProjectIssues_page projectName nextCursor (List.append acc issueData)
        | None -> List.append acc issueData

    getProjectIssues_page projectName None []
    |> List.map fst
