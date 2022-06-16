module NowWhat.CLI

open NowWhat.API
open NowWhat.Domain

let nowwhat argv =
  try
    printfn "Now what?"

    let gitHubIssues = Github.getProjectIssuesMock "Project Tracker"
    printfn $"Number of issues in GitHub: {gitHubIssues |> Seq.length}"

    let forecastProjects = Forecast.getProjects ()
    printfn $"Number of projects in Forecast: {forecastProjects |> Seq.length}"

    let projects = Project.constructProjects forecastProjects gitHubIssues
    printfn $"Number of constructed projects: {projects |> Seq.length}"
    0

  with
    | Forecast.UnauthorisedException(string) ->
      printfn $"Error in Forecast authorisation: {string}"
      -1
    | Forecast.FailedException(string) ->
      printfn $"Error retrieving Forecast data: {string}"
      -2
    | Config.LoadException(string) ->
      printfn $"Error loading secrets: {string}"
      -3
    | other ->
      printfn $"Other exception: {other}"
      -4
