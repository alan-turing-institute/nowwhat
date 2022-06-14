module NowWhat.CLI

open NowWhat.API
open NowWhat.Config

let nowwhat argv =
    printfn "Now what?"

    let secrets = getSecrets ()
    printfn $"Secrets: {secrets}"

    let githubIssues =
        Github.ProjectBoard
        |> Github.getAllProjectIssues secrets.githubToken
        |> snd
        |> Array.filter (fun (_, _, status, _) -> status = "OPEN")
    printfn "Number of open issues in Project tracker: %d" githubIssues.Length

    let forecastProjects = Forecast.getProjects secrets.forecastId secrets.forecastToken
    printfn "Number of projects in Forecast: %d" (forecastProjects.Projects |> Seq.length)

    0
