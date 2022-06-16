module NowWhat.CLI

open NowWhat.API

let nowwhat argv =
  try
    printfn "Now what?"

    let gitHubIssues = Github.getProjectIssues "Project Tracker"
    printfn $"Number of issues in GitHub: {gitHubIssues |> Seq.length}"

    let forecastProjects = Forecast.getProjects ()
    printfn $"Number of projects in Forecast: {forecastProjects |> Seq.length}"

    // for p in forecastProjects do
    //     printfn $"{p}"

    0

  with
    | Forecast.UnauthorisedException(string) ->
      printfn $"Error in Forecast authorisation: {string}"
      -1
    | Forecast.FailedException(string) ->
      printfn $"Error retrieving Forecast data: {string}"
      -2
    | Config.SecretLoadException(string) ->
      printfn $"Error loading secrets: {string}"
      -3
    | other ->
      printfn $"Other exception: {other}"
      -4
