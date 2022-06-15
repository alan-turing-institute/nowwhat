module NowWhat.CLI

open NowWhat.API
open NowWhat.Config

let nowwhat argv =
  try
    printfn "Now what?"
    let githubIssues =
        GithubAPI.ProjectBoard
        |> GithubAPI.getAllProjectIssues
        |> snd
        |> Array.filter (fun (_, _, status, _) -> status = "OPEN")
    printfn "Number of open issues in Project tracker: %d" githubIssues.Length

    let forecastProjects = ForecastAPI.getProjects2()
    printfn $"Number of projects in Forecast: {forecastProjects |> Seq.length}"

    0

  with
    | ForecastAPI.UnauthorisedException(string) ->
      printfn $"Error in Forecast authorisation: {string}"
      -1
    | ForecastAPI.FailedException(string) ->
      printfn $"Error retrieving Forecast data: {string}"
      -2
    | Config.SecretLoadException(string) ->
      printfn $"Error loading secrets: {string}"
      -3
    | other ->
      printfn $"Other exception: {other}"
      -4
