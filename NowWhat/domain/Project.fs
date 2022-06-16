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

// let justInt (intCode : int option) : int =
//    match intCode with
//     | Some int -> int
//     | None -> None


let constructProjects (forecastProjects : Forecast.Project List) (githubIssues : Github.Issue List) : (Project list) = //*(Forecast.Project list)*(Github.Issue list)=
  // Join on Forecast.code == Issue.number
  // let forecastTuples = forecastProjects
  //                       |> List.map ( fun project -> ((maybeProjectCode project.code), project) )
  //                       |> List.map (
  //                         fun projectTuple -> match projectTuple with
  //                                             | (Some code, project) -> Some ( code, project )
  //                                             | (None, _) -> None
  //                       )
  //                       |> List.choose id

  let forecastTuples = forecastProjects
                        |> List.map ( fun project -> match ((maybeProjectCode project.code), project) with
                                                     | (Some code, project) -> Some (code, project)
                                                     | (None, _) -> None
                        )
                        |> List.choose id
  let forecastMap = forecastTuples |> Map // maybe this will break on duplicates?
  let forecastIds = forecastMap.Keys |> Set

  printfn "%A" forecastMap

  let githubTuples = githubIssues
                      |> List.map ( fun issue ->  (issue.number, issue) )
  let githubMap = githubTuples |> Map // maybe this will break on duplicates?
  let githubIds = githubMap.Keys |> Set

  printfn "%A" githubMap

  // let match = forecastIds::intersect githubIds

  let matchedIds = Set.intersect (githubMap.Keys |> Set) (forecastMap.Keys |> Set) |> Set.toList

  printfn "%A" matchedIds


  let matches = Set.intersect (githubMap.Keys |> Set) (forecastMap.Keys |> Set)
                |> Set.toList

  []
