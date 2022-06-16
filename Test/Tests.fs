module Tests

open System
open System.IO
open Xunit
open NowWhat.CLI
open NowWhat.API
open Thoth.Json.Net

let fixtureDir: string = $"{__SOURCE_DIRECTORY__}/fixtures"

type RedirectStdOut(fileNameStub: string) =
    // Store native stdout for later use
    let consoleStdOut = Console.Out
    let capturedStdOut = new StreamWriter($"{fixtureDir}/{fileNameStub}.new.txt")

    // Setup: redirect output to a StringWriter
    do Console.SetOut(capturedStdOut)

    // Teardown: Reset to the cached native stdout
    interface IDisposable with
        member __.Dispose () =
            capturedStdOut.Flush()
            Console.SetOut(consoleStdOut)

let expectStdOut (fileNameStub: string): unit =
    let expectedPath = $"{fixtureDir}/{fileNameStub}.txt"
    let actualPath = $"{fixtureDir}/{fileNameStub}.new.txt"
    Assert.Equal(File.ReadAllText(expectedPath), File.ReadAllText(actualPath))
    // We will only delete the file and return 'true' if the previous Assert succeeded
    File.Delete(actualPath)

[<Theory>]
[<InlineData("withEnvVars")>]
let ``End-to-end test with environment variables`` (fileNameStub: string): unit =
    using (new RedirectStdOut(fileNameStub)) ( fun _ ->
        nowwhat ()
    ) |> ignore
    expectStdOut fileNameStub

[<Theory>]
[<InlineData("rootSerialised.json")>]
let ``test Forecast JSON deserialisation`` (jsonFileName: string): unit =
    let expected =  { Forecast.Root.projects = [{ id = 1684536; harvestId = None; clientId = None; name = "Time Off"; code = None; tags = []; notes = None }] }
    let rootJson = File.ReadAllText($"{fixtureDir}/{jsonFileName}")
    let actual = match rootJson |> Decode.fromString Forecast.rootDecoder with
                 | Ok projects -> projects
                 | Error _ -> failwith "Forecast root does not deserialise"
    Assert.Equal(expected, actual)

[<Theory>]
[<InlineData("GithubProjectsSerialised.json")>]
let ``test Github Project Columns JSON deserialisation`` (jsonFileName: string): unit =
    let expected = { Github.ProjectRoot.projects = [{number = 2; name="Project Tracker"; columns=[{name="Suggested"}]}]}
    let rootJson = String.Join("", File.ReadAllLines($"{__SOURCE_DIRECTORY__}/fixtures/{jsonFileName}"))
    // printfn $"Expected Issue: \n{expected}"
    let actual = match rootJson |> Decode.fromString Github.projectRootDecoder with
                  | Ok projects -> projects
                  | Error _ -> failwith "Project Root does not deserialise"

    Assert.Equal(expected, actual)
