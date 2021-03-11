// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open NowWhat.API

let checkEnvironmentVariables () =
  let env = System.Environment.GetEnvironmentVariables()

  (true, ["GITHUBID"; "GITHUBTOKEN"; "FORECASTID"; "FORECASTTOKEN"])
  ||> List.fold (fun allExist variable -> 
    if not (env.Contains variable) then 
      printfn $"Please set environment variable {variable}."
      false
    else
      allExist)

[<EntryPoint>]
let main argv =
    printfn "Now what?"

    if checkEnvironmentVariables() then

      let githubIssues = 
        Github.ProjectBoard
        |> Github.getAllProjectIssues
        |> snd
        |> Array.filter (fun (number, title, status, _) -> status = "OPEN")

      printfn "Number of open issues in Project tracker: %d" githubIssues.Length

      let forecastProjects = Forecast.getProjects ()
      printfn "Number of projects in Forecast: %d" (forecastProjects.Projects |> Seq.length)

    0 // return an integer exit code