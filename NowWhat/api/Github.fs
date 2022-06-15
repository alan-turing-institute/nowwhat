module NowWhat.API.Github

open HttpFs.Client
open Hopac
open FSharp.Data
open Thoth.Json.Net
open NowWhat.Config

(* ---------------------------------------------------------------------------------------------------

   GitHub data model

*)

type Column = {
  name : string
}

type Project = {
  number: int;
  name: string;
  columns: Column List;
}

type ProjectRoot = {
  projects: Project List
}

/// There are currently **two** Issue types -- a placeholder (Issue) allowing work to continue on
/// the business/validation logic, and a WIP version (Issue_WIP) deserialised from the GraphQL API,
/// which will eventually replace Issue.
type Issue_WIP = {
  number: int;
}

type Issue = {
  id: string
  number: int
  title: string
  body: string
  state: string
}

// Can we elide this?
type IssueRoot = {
  issue: Issue_WIP;
}

(* ---------------------------------------------------------------------------------------------------
   Get Forecast objects as F# types
   *)

let columnDecoder : Decoder<Column> =
    Decode.object (
        fun get -> {
          Column.name = get.Required.At ["node"; "name"] Decode.string
        }
    )

let projectDecoder : Decoder<Project> =
    Decode.object (
        fun get -> {
          Project.number = get.Required.At ["node"; "number"] Decode.int
          Project.name = get.Required.At ["node"; "name"] Decode.string
          Project.columns = get.Required.At ["node";"columns"; "edges"] (Decode.list columnDecoder)
        }
    )

let projectRootDecoder : Decoder<ProjectRoot> =
    Decode.object (
        fun get -> {
          ProjectRoot.projects = get.Required.At ["data"; "repository"; "projects"; "edges"] (Decode.list projectDecoder)
        }
    )

let issueDecoder : Decoder<Issue_WIP> =
    Decode.object (
        fun get -> {
            Issue_WIP.number = get.Required.Field "number" Decode.int
            // Issue.url= get.Required.Field "url" Decode.string;
        }
    )

let issueRootDecoder : Decoder<IssueRoot> =
  Decode.object (
    fun get -> {
      IssueRoot.issue = get.Required.At ["data"; "repository"; "issue"] issueDecoder
    }
  )

(* ---------------------------------------------------------------------------------------------------

   Interface to the GitHub API

*)

type ProjectIssuesFromGraphQL = JsonProvider<"api/sample-json/gh-project-issues.json">
type IssueDetailsFromGraphQL = JsonProvider<"api/sample-json/gh-issue-details.json">

let [<Literal>] GithubGraphQLEndpoint = "https://api.github.com/graphql"

let [<Literal>] ProjectBoard = "Project Tracker"
let [<Literal>] StandingRoles = "Standing Roles"
let allProjectBoards = [
  ProjectBoard
  StandingRoles
]

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

let getAllProjectIssues (projectName: string): Issue array =
  // the parent function is only wrapping up the recursive call that deals with paging of the responses
  let githubToken = match getSecrets () with
                    | Ok secrets -> secrets.githubToken
                    | Error err -> raise err

  let rec getProjectIssues projectName cursor acc =
    let queryTemplate = System.IO.File.ReadAllText $"{__SOURCE_DIRECTORY__}/queries/issues-by-project-graphql.json"

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

    // parse the response using the type provider
    let issues = ProjectIssuesFromGraphQL.Parse result
    // run WIP implementation in parallel with old until migration-time
    let issues2 = result |> Decode.fromString projectRootDecoder
    let issueData =
      issues.Data.Repository.Projects.Edges
      |> Array.exactlyOne
      |> fun project ->
        let projectName = project.Node.Name, project.Node.Number
        let cards: (Issue * string) array =
          project.Node.Columns.Edges
          |> Array.collect (fun c -> c.Node.Cards.Edges |> Array.map (fun x -> ({
            id = x.Node.Id;
            number = x.Node.Content.Number;
            title = x.Node.Content.Title;
            body = x.Node.Content.Body;
            state = x.Node.Content.State
          }, x.Cursor)) )
        cards

    // Cursor points to the last item returned, used for paging of the requests
    let nextCursor =
        if issueData.Length = 0 then
          None
        else
          issueData
          |> Array.last
          |> fun (_, c) -> Some c

    match nextCursor with
    | Some _ -> getProjectIssues projectName nextCursor (Array.append acc issueData)
    | None -> Array.append acc issueData

  getProjectIssues projectName None [||] |> Array.map fst

// Currently unused
let getIssueDetails (gitHubToken: string) issueNumber =
    let queryTemplate = System.IO.File.ReadAllText "api/queries/issue-details-graphql.json"

    // fill in placeholders into the query - issue number
    let query =
      queryTemplate.Replace("ISSUENUMBER", $"{issueNumber}")
      |> formatQuery

    let result = runGithubQuery gitHubToken query

    // parse the response using the type provider
    let issues = IssueDetailsFromGraphQL.Parse result
    issues.Data.Repository.Issue


(* ---------------------------------------------------------------------------------------------------
   Public interface to this module
*)

let getIssues (): Issue list =
  getAllProjectIssues "Project Tracker" |> Array.toList
