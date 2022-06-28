module NowWhat.Schedule

(*  Interface to domain data

    Obtains a complete schedule by unifying data from GitHub and Forecast
    Along the way:
    - emits errors (and stops) for things that prevent further processing;
    - emits warnings for other problems

    *)
    
open System.Text.RegularExpressions

open NowWhat.API


type Project = { id: int }

type Schedule = { projects: Project list }

/// The single hard-coded project
let [<Literal>] TimeOffProjectId = 1684536

/// Regular expression to match valid project codes
let [<Literal>] rxCode = "^hut23-(\d+)$"


// CHECK: Check that every project has an issue number of the correct form.

let validateProject (p : Forecast.Project) : Result<Project, Forecast.Project> =
    match p.Code with
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
        |> Async.RunSynchronously
        |> List.filter (fun p -> (not p.IsArchived) && (p.Id <> TimeOffProjectId))
        |> List.fold
            (fun (okProjects, errorProjects) project ->
                match validateProject project with
                | Ok p -> (p :: okProjects, errorProjects)
                | Error p -> (okProjects, p :: errorProjects)
                )
            ([], [])      
  
    let badProjects = snd forecastProjects
    if List.isEmpty badProjects then
        fst forecastProjects
    else
        printfn "Error: Some Forecast projects have malformed project codes:"
        let ppCode cd =
            match cd with
                | None -> ""
                | Some c -> string c

        badProjects
        |> List.map (fun p -> $"{p.Id} [{ppCode p.Code}] \"{p.Name}\"")
        |> String.concat "\n"
        |> printfn "%s"

        failwith "Can't decode all projects."
