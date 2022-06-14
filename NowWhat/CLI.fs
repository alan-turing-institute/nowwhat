module NowWhat.CLI

open NowWhat.API
open NowWhat.Config

let nowwhat argv =
  try
    printfn "Now what?"

    let secrets = getSecrets ()

    let githubIssues =
        Github.ProjectBoard
        |> Github.getAllProjectIssues secrets.githubToken
        |> snd
        |> Array.filter (fun (_, _, status, _) -> status = "OPEN")
    printfn "Number of open issues in Project tracker: %d" githubIssues.Length

    let forecastProjects = Forecast.getProjects()
    printfn $"Number of projects in Forecast: {(forecastProjects.Projects |> Seq.length)}"

    0
  with
    | Forecast.UnauthorisedException(string) -> 
      printfn $"ERROR: {string}"
      -1
    | _ -> -2

