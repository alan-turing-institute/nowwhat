module NowWhat.CLI

open NowWhat.API

let envVars = {|
  gitHub = "NOWWHAT_GITHUB_TOKEN";
  forecastId = "FORECASTID";
  forecastToken = "FORECASTTOKEN"
|}

let gitHubVars = [envVars.gitHub]
let forecastVars = [envVars.forecastId; envVars.forecastToken]

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

    if checkEnvironmentVariables gitHubVars then
      let gitHubToken = System.Environment.GetEnvironmentVariable(envVars.gitHub)
      let githubIssues =
        Github.ProjectBoard
        |> Github.getAllProjectIssues gitHubToken
        |> snd
        |> Array.filter (fun (_, _, status, _) -> status = "OPEN")
      printfn "Number of open issues in Project tracker: %d" githubIssues.Length

    if checkEnvironmentVariables forecastVars then
      let forecastProjects = Forecast.getProjects ()
      printfn "Number of projects in Forecast: %d" (forecastProjects.Projects |> Seq.length)

    0
