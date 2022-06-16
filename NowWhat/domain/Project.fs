module NowWhat.Domain.Project

open NowWhat.API
open System

type TimeConstraint = {
  earliestStartDate: DateOnly
  latestStartDate: DateOnly
  latestEndDate: DateOnly
  totalResourceWeeks: float
  runRateMin: float
  runRateNominal: float
  runRateMax: float
  financeCode: string option
}

type Project = {
  forecastId: int
  githubIssue: int
  name: string
  constraints: TimeConstraint list
}

let maybeProjectCode (code : string option) : int option =
  try
    match code with
    | Some string -> Some (string.Replace("hut23-", "") |> int)
    | None -> None
  with
    | error -> printfn $"Error: {error}"; None


let constructProjects (forecastProjects : Forecast.Project List) (githubIssues : Github.Issue List) : (Project list) = //*(Forecast.Project list)*(Github.Issue list)=

  let forecastMap =
      forecastProjects
      |> List.choose (fun project ->
           Option.map (fun code -> (code, project)) (maybeProjectCode project.code))
      |> Map

  let githubMap =
      githubIssues
      |> List.map ( fun issue -> (issue.number, issue) )
      |> Map

  let projects =
      forecastMap.Keys
      |> Seq.choose (fun forecastKey  ->
          if githubMap.ContainsKey forecastKey then
                  let g = githubMap.[forecastKey]
                  let f = forecastMap.[forecastKey]
                  Some {
                    forecastId = forecastKey
                    githubIssue = g.number
                    name = g.title
                    constraints = []
                  }
          else None
          )
      |> List.ofSeq

  projects
