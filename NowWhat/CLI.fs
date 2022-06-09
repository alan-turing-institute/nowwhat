module NowWhat.CLI

open NowWhat.API

let gitHubVars = ["GITHUBID"; "GITHUBTOKEN"]
let forecastVars = ["FORECASTID"; "FORECASTTOKEN"]

let checkEnvironmentVariables (envVars: string List) =
  let env = System.Environment.GetEnvironmentVariables()

  (true, envVars)
  ||> List.fold (fun allExist variable ->
    if not (env.Contains variable) then
      printfn $"Please set environment variable {variable}."
      false
    else
      allExist)

let nowwhat argv =
    printfn "Now what?"

    if checkEnvironmentVariables (gitHubVars @ forecastVars) then

      let githubIssues =
        Github.ProjectBoard
        |> Github.getAllProjectIssues
        |> snd
        |> Array.filter (fun (number, title, status, _) -> status = "OPEN")

      printfn "Number of open issues in Project tracker: %d" githubIssues.Length

      let forecastProjects = Forecast.getProjects ()
      printfn "Number of projects in Forecast: %d" (forecastProjects.Projects |> Seq.length)

    0
