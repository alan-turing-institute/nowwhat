module NowWhat.CLI

open NowWhat.API

// This doesn't really do what it claims to, feel free to improve
let roundToNearest (n: int) (roundTo: int): int =
  n / roundTo * roundTo

let nowwhat argv =
  try
    printfn "Now what?"

    let gitHubIssues = Github.getIssues ()
    printfn $"Number of issues in GitHub: over {roundToNearest (gitHubIssues |> Map.values |> Seq.map Seq.length |> Seq.sum) 100}"

    let forecastProjects = Forecast.getProjects ()
    printfn $"Number of projects in Forecast: over {roundToNearest (forecastProjects |> Seq.length) 100}"

    // for p in forecastProjects do
    //     printfn $"{p}"

    0

  with
    | ForecastRaw.UnauthorisedException(string) ->
      printfn $"Error in Forecast authorisation: {string}"
      -1
    | ForecastRaw.FailedException(string) ->
      printfn $"Error retrieving Forecast data: {string}"
      -2
    | Config.LoadException(string) ->
      printfn $"Error loading secrets: {string}"
      -3
    | other ->
      printfn $"Other exception: {other}"
      -4
