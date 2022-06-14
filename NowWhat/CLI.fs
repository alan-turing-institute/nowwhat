module NowWhat.CLI

open NowWhat.API
open NowWhat.Config

let nowwhat argv =
  printfn "Now what?"

  let secrets = match getSecrets () with
                | Ok secrets -> secrets
                | Error err -> raise (Forecast.UnauthorisedException(err.ToString()))

  let githubIssues =
      Github.ProjectBoard
      |> Github.getAllProjectIssues secrets.githubToken
      |> snd
      |> Array.filter (fun (_, _, status, _) -> status = "OPEN")
  printfn "Number of open issues in Project tracker: %d" githubIssues.Length

  let forecastProjects = Forecast.getProjects()
  printfn $"Number of projects in Forecast: {(forecastProjects.Projects |> Seq.length)}"

  0
