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
  forecastIds: int list
  githubIssue: int
  name: string
  constraints: TimeConstraint list
}

let maybeProjectCode (code : string option) : int option =
  try
    match code with
    | Some s ->
        try 
           s.Replace("hut23-", "") |> int |> Some
        with _ ->
           None
    | None -> None
  with
    | error ->
        printfn "The string cannot be parsed: %A" code
        printfn $"Error: {error}"
        None


let constructProjects (forecastProjects : Forecast.Project List) (githubIssues : Github.Issue List) : (Project list) = //*(Forecast.Project list)*(Github.Issue list)=

  let forecastMap =
      forecastProjects
      |> List.choose (fun project ->
           Option.map (fun code -> (code, project)) (maybeProjectCode project.code))
      |> List.groupBy fst
      |> List.map (fun (code, values) -> code, values |> List.map snd)
      |> Map

  let githubMap =
      githubIssues
      |> List.map ( fun issue -> (issue.number, issue) )
      |> Map

  let projects =
      forecastMap.Keys
      |> Seq.choose (fun projectId  ->
          if githubMap.ContainsKey projectId then
                  let g = githubMap.[projectId]
                  let f = forecastMap.[projectId]
                     
                  Some {
                    forecastIds = forecastMap.[projectId] |> List.map (fun fp -> fp.id)
                    githubIssue = g.number
                    name = g.title
                    constraints = []
                  }
          else None
          )
      |> List.ofSeq

  projects
