module NowWhat.CLI

open NowWhat.API
open NowWhat.Config

let nowwhat argv =
  try
    printfn "Now what?"
    let githubIssues =
        Github.ProjectBoard
        |> Github.getAllProjectIssues
        |> snd
        |> Array.filter (fun (_, _, status, _) -> status = "OPEN")
    printfn "Number of open issues in Project tracker: %d" githubIssues.Length

    let forecastProjects = Forecast.getProjects()
    printfn $"Number of projects in Forecast: {(forecastProjects.Projects |> Seq.length)}"

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
    | _ -> -4
