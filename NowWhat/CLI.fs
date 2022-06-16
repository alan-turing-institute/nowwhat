module NowWhat.CLI

open NowWhat.API

// This doesn't really do what it claims to, feel free to improve
let roundToNearest (n: int) (roundTo: int): int =
  n / roundTo * roundTo

let nowwhat argv =
  try
    printfn "Now what?"

    let gitHubIssues = Github.getProjectIssues "Project Tracker"
    printfn $"Number of issues in GitHub: over {roundToNearest (gitHubIssues |> Seq.length) 100}"

    let forecastProjects = Forecast.getProjects ()
    printfn $"Number of projects in Forecast: over {roundToNearest (forecastProjects |> Seq.length) 100}"

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
