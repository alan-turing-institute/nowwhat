module NowWhat.Schedule

(*  Interface to domain data

    Obtains a complete schedule by unifying data from GitHub and Forecast
    1. Check Forecast projects for internal consitency
    2. Check GitHub projects for internal completeness
    3. Check both projects together for consistency and completeness

    Along the way, emit:
    - Panics (and stop) for things that prevent further processing;
    - Errors: for serious problems
    - Warnings: for other issues

    *)
    
open System.Text.RegularExpressions

open NowWhat.API


type Project =
    { id: int
  }   

type Schedule = { projects: Project list }

/// The single hard-coded Forecast project
let [<Literal>] TimeOffProjectId = 1684536

/// Regular expression to match valid project codes
let [<Literal>] rxCode = "^hut23-(\d+)$"

// CHECK: Check that every project has an issue number of the correct form.




let validateProject (p : Forecast.Project) : Result<Project, Forecast.Project> =
    match p.code with
        | Some code ->
            let m = Regex.Match(code, rxCode) 
            if m.Groups.Count = 2 then
                Ok { id = int m.Groups.[1].Value } 
            else
                Error p
        | None ->
            Error p

/// Obtain and validate projects from Forecast.
/// Excludes archived projects and the "Time Off" hardcoded project.
let getProjects () =
    // Accumulate a pair of (valid projects, invalid projects)
    let forecastProjects =
        Forecast.getProjects ()
        |> List.filter (fun p -> not p.isArchived)
        |> List.filter (fun p -> p.id <> TimeOffProjectId)
        |> List.fold
            (fun (goodProjects, badProjects) project ->
                match validateProject project with
                | Ok p -> (p :: goodProjects, badProjects)
                | Error p -> (goodProjects, p :: badProjects))
            ([], [])
            
    let (goodProjects, badProjects) = forecastProjects
    if List.isEmpty badProjects then
        goodProjects
    else
        printfn "Error: Some Forecast projects have malformed project codes:"
        let ppCode cd =
            match cd with
                | None -> ""
                | Some c -> string c

        badProjects
        |> List.map (fun p -> $"{p.id} [{ppCode p.code}] \"{p.name}\"")
        |> String.concat "\n"
        |> printfn "%s"

        failwith "Can't decode all projects."
