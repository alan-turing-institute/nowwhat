module Tests

open System
open System.IO
open Xunit
open NowWhat.CLI
open NowWhat.API
open NowWhat.DomainModel
open Thoth.Json.Net



type RedirectStdOut(fileNameStub: string) =
    // Store native stdout for later use
    let consoleStdOut = Console.Out
    let capturedStdOut = new StreamWriter($"{__SOURCE_DIRECTORY__}/fixtures/{fileNameStub}.new.txt")

    // Setup: redirect output to a StringWriter
    do Console.SetOut(capturedStdOut)

    // Teardown: Reset to the cached native stdout
    interface IDisposable with
        member __.Dispose () =
            capturedStdOut.Flush()
            Console.SetOut(consoleStdOut)

let StdOutMatches (fileNameStub: string) =
    let expectedPath = $"{__SOURCE_DIRECTORY__}/fixtures/{fileNameStub}.txt"
    let actualPath = $"{__SOURCE_DIRECTORY__}/fixtures/{fileNameStub}.new.txt"
    Assert.Equal(File.ReadAllText(expectedPath), File.ReadAllText(actualPath))
    // We will only delete the file and return 'true' if the previous Assert succeeded
    File.Delete(actualPath)
    true

[<Theory>]
[<InlineData("withEnvVars")>]
let ``End-to-end test with environment variables`` (fileNameStub: string) =
    using (new RedirectStdOut(fileNameStub)) ( fun _ ->
        nowwhat ()
    ) |> ignore
    Assert.True(StdOutMatches(fileNameStub))

[<Theory>]
[<InlineData("rootSerialised.json")>]
let ``test Forecast JSON deserialisation`` (jsonFileName: string) =
    let expected =  { ForecastModel.Root.projects = [{ id = 1684536; name = "Time Off"; color = "black"; code = None; notes = None }] }
    // printfn $"Expected Issue: \n{expected}"
    let rootJson = String.Join("", File.ReadAllLines($"{__SOURCE_DIRECTORY__}/fixtures/{jsonFileName}"))
    let actual = match rootJson |> Decode.fromString ForecastModel.rootDecoder with
                 | Ok projects -> projects
                 | Error _ -> { ForecastModel.Root.projects = [] }
    Assert.Equal(expected, actual)

[<Theory>]
[<InlineData("issueSerialised.json")>]
let ``test Github Issue JSON deserialisation`` (jsonFileName: string) =
    let expected = { GithubModel.IssueRoot.issue = {number = 1; }}
    let rootJson = String.Join("", File.ReadAllLines($"{__SOURCE_DIRECTORY__}/fixtures/{jsonFileName}"))
    // printfn $"Expected Issue: \n{expected}"
    let actual = match rootJson |> Decode.fromString GithubModel.issueRootDecoder with
                  | Ok issue -> issue
                  | Error _ -> failwith "Issue Does not deserialise "
    
    Assert.Equal(expected, actual)

// [<Theory>]
// [<InlineData("GithubProjectSerialised.json")>]
// let ``test Github Project JSON deserialisation`` (jsonFileName: string) =
//     let expected = { GithubModel.Project.name="Project Tracker"; GithubModel.Project.number=2 }
//     let rootJson = String.Join("", File.ReadAllLines($"{__SOURCE_DIRECTORY__}/fixtures/{jsonFileName}"))
//     // printfn $"Expected Issue: \n{expected}"
//     let actual = match rootJson |> Decode.fromString GithubModel.projectDecoder with
//                   | Ok project -> project
//                   | Error _ -> failwith "Project Does not deserialise "
    
//     Assert.Equal(expected, actual)

// [<Theory>]
// [<InlineData("GithubProjectsSerialised.json")>]
// let ``test Github Projects JSON deserialisation`` (jsonFileName: string) =
//     let expected = { GithubModel.ProjectRoot.projects = [{number = 2; name="Project Tracker"}]}
//     let rootJson = String.Join("", File.ReadAllLines($"{__SOURCE_DIRECTORY__}/fixtures/{jsonFileName}"))
//     // printfn $"Expected Issue: \n{expected}"
//     let actual = match rootJson |> Decode.fromString GithubModel.projectRootDecoder with
//                   | Ok projects -> projects
//                   | Error _ -> failwith "Project Does not deserialise "
    
//     Assert.Equal(expected, actual)

[<Theory>]
[<InlineData("GithubProjectsSerialised.json")>]
let ``test Github Project Columns JSON deserialisation`` (jsonFileName: string) =
    let expected = { GithubModel.ProjectRoot.projects = [{number = 2; name="Project Tracker"; columns=[]}]}
    let rootJson = String.Join("", File.ReadAllLines($"{__SOURCE_DIRECTORY__}/fixtures/{jsonFileName}"))
    // printfn $"Expected Issue: \n{expected}"
    let actual = match rootJson |> Decode.fromString GithubModel.projectRootDecoder with
                  | Ok projects -> projects
                  | Error _ -> failwith "Project Root Does not deserialise "
    
    Assert.Equal(expected, actual)
