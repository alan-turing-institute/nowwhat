module NowWhat.API.Github

open HttpFs.Client
open Hopac
open FSharp.Data

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

let getAllProjectIssues (gitHubToken: string) projectName =
  // the parent function is only wrapping up the recursive call that deals with paging of the responses

  let rec getProjectIssues projectName cursor acc =
    let queryTemplate = System.IO.File.ReadAllText "../../../../NowWhat/api/queries/issues-by-project-graphql.json"

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

    let result = runGithubQuery gitHubToken query

    // parse the response using the type provider
    let issues = ProjectIssuesFromGraphQL.Parse result
    let proj, issueData =
      issues.Data.Repository.Projects.Edges
      |> Array.exactlyOne
      |> fun project ->
        let projectName = project.Node.Name, project.Node.Number
        let cards =
          project.Node.Columns.Edges
          |> Array.collect (fun c -> c.Node.Cards.Edges |> Array.map (fun x -> x.Node.Content.Number, x.Node.Content.Title, x.Node.Content.State, x.Cursor) )
          // TODO: Collect results into some reasonable type instead of a tuple
        projectName, cards

    // Cursor points to the last item returned, used for paging of the requests
    let nextCursor =
        if issueData.Length = 0 then
          None
        else
          issueData
          |> Array.last
          |> fun (number, title, state, c) -> Some c

    match nextCursor with
    | Some _ -> getProjectIssues projectName nextCursor (Some proj, Array.append (snd acc) issueData)
    | None -> (Some proj, Array.append (snd acc) issueData)

  getProjectIssues projectName None (None, [||])

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

