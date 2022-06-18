module NowWhat.API.Github

open HttpFs.Client
open Hopac
open Thoth.Json.Net

open NowWhat.Config

[<Literal>]
let GithubGraphQLEndpoint = "https://api.github.com/graphql"

[<Literal>]
let ProjectBoard = "Project Tracker"



(* -----------------------------------------------------------------------------
   GitHub GraphQL data model
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
   JSON decoder utilities
*)

let issueDecoder: Decoder<Issue * string> =
    Decode.object (fun get ->
        ({ Issue.number = get.Required.At [ "node"; "content"; "number" ] Decode.int
           Issue.title  = get.Required.At [ "node"; "content"; "title" ] Decode.string
           Issue.body   = get.Required.At [ "node"; "content"; "body" ] Decode.string
           Issue.state  = get.Required.At [ "node"; "content"; "state" ] Decode.string },
         get.Required.Field "cursor" Decode.string))

let columnDecoder: Decoder<Column> =
    Decode.object (fun get ->
        { Column.name  = get.Required.At [ "node"; "name" ] Decode.string
          Column.cards = get.Required.At [ "node"; "cards"; "edges" ] (Decode.list issueDecoder) })

let projectDecoder: Decoder<Project> =
    Decode.object (fun get ->
        { Project.number  = get.Required.At [ "node"; "number" ] Decode.int
          Project.name    = get.Required.At [ "node"; "name" ] Decode.string
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
   GitHub API
*)


/// Query Github GraphQL endpoint
/// body should be a json-encoded GraphQL query element
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

let getProjectIssues (tkn: string) (projectName: string) : Map<string, Issue list> =
    // Wrap up the recursive call that deals with paging of the responses

    // Well, this is a bodge. The GraphQL query is its own language. We load a
    // template query from a file and then replace certain keywords with
    // parameters.
    let baseQueryTemplate =
        System.IO.File.ReadAllText $"{__SOURCE_DIRECTORY__}/queries/issues-by-project-graphql.json"

    let queryTemplate =
        baseQueryTemplate.Replace("PROJECTNAME", $"\\\"{projectName}\\\"")

    // Get the next set of issues (from curso) from GitHub
    let downloadIssues cursor =
        // Replace placeholders in query and make GraphQL request 
        let result =
            match cursor with
            | None ->
                // Get initial batch
                queryTemplate.Replace("CURSOR", "null")
            | Some crs ->
                // Get subsequent batches
                queryTemplate.Replace("CURSOR", $"\\\"{crs}\\\"")
            |> formatQuery
            |> (runGithubQuery tkn)

        // Decode returned JSON
        match Decode.fromString projectRootDecoder result with
            | Ok root -> root.projects.Head.columns
            | Error err -> failwith ("Failed to decode GraphQL response from GitHub:\n" + err)
        

    let rec getProjectIssues_page cursor (acc: Map<string, Issue list>) =
        // Add the issues in each column to the column map, keeping track of (a)
        // how many issues we've seen (to know when to stop paging) and (b) the
        // last cursor.
        let updateColMap (colMap, countOfIssues, lastCursor) column =
            match column.cards with
                | [] ->
                    (colMap, countOfIssues, lastCursor)
                | cards ->
                    let issues = List.map (fun (issue, _) -> issue) cards  

                    (Map.change
                        column.name
                        (fun value ->
                             match value with
                             | None -> Some issues
                             | Some is -> Some (List.append issues is))
                         colMap,
                     countOfIssues + List.length issues,
                     Some (snd (List.last cards))
                    ) 

        // columns is a list of Column, and a Column is a list of (issue,
        // cursor). We want the last cursor. The last page has
        // been returned when all the columns are the empty list.
        let (colMap, N, crs) =
            List.fold updateColMap (acc, 0, cursor) (downloadIssues cursor)

        if N = 0 then
            colMap
        else
            getProjectIssues_page crs colMap

    getProjectIssues_page None Map.empty
    

(* -----------------------------------------------------------------------------
  External API
*)

/// Return a list of columns, each with a list of issues
let getIssues () =
    let githubToken = (getConfig ()).githubToken
    getProjectIssues githubToken ProjectBoard
