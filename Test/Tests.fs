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
    let capturedStdOut = new StreamWriter($"{__SOURCE_DIRECTORY__}/expected/{fileNameStub}.new.txt")

    // Setup: redirect output to a StringWriter
    do Console.SetOut(capturedStdOut)

    // Teardown: Reset to the cached native stdout
    interface IDisposable with
        member __.Dispose () =
            capturedStdOut.Flush()
            Console.SetOut(consoleStdOut)

let StdOutMatches (fileNameStub: string) =
    let expectedPath = $"{__SOURCE_DIRECTORY__}/expected/{fileNameStub}.txt"
    let actualPath = $"{__SOURCE_DIRECTORY__}/expected/{fileNameStub}.new.txt"
    Assert.Equal(File.ReadAllText(expectedPath), File.ReadAllText(actualPath))
    // We will only delete the file and return 'true' if the previous Assert succeeded
    File.Delete(actualPath)
    true

let rootJson = """{
  "projects": [
    {
      "id": 1684536,
      "name": "Time Off",
      "color": "black",
      "code": null,
      "notes": null,
      "start_date": "2020-01-07",
      "end_date": "2020-01-07",
      "harvest_id": null,
      "archived": false,
      "updated_at": "2021-01-11T14:13:48.634Z",
      "updated_by_id": 867021,
      "client_id": null,
      "tags": []
    }
  ]
}
"""

// [<Theory>]
// [<InlineData("withEnvVars")>]
// let ``End-to-end test with environment variables`` (fileNameStub: string) =
//     using (new RedirectStdOut(fileNameStub)) ( fun _ ->
//         nowwhat ()
//     ) |> ignore
//     Assert.True(StdOutMatches(fileNameStub))

[<Theory>]
[<InlineData("noEnvVars")>]
let ``End-to-end test without environment variables`` (fileNameStub: string) =
    let testfn () =
        using (new RedirectStdOut(fileNameStub)) ( fun _ ->
            let forecastId = Environment.GetEnvironmentVariable("FORECAST_ID")
            let forecastToken = Environment.GetEnvironmentVariable("NOWWHAT_FORECAST_TOKEN")
            try
                for envVar in [ "FORECAST_ID"; "NOWWHAT_FORECAST_TOKEN" ] do
                    Environment.SetEnvironmentVariable(envVar, "")
                    nowwhat () |> ignore
            finally
                Environment.SetEnvironmentVariable("FORECAST_ID", forecastId)
                Environment.SetEnvironmentVariable("NOWWHAT_FORECAST_TOKEN", forecastToken)
        )
    Assert.Throws<Forecast.UnauthorisedException>(testfn) |> ignore
    Assert.True(StdOutMatches(fileNameStub))

[<Fact>]
let ``test Forecast JSON deserialisation`` () =
    let expected =  { Forecast.Root.projects = [{ id = 1684536; name = "Time Off"; color = "black"; code = None; notes = None }] }
    let actual = match rootJson |> Decode.fromString Forecast.rootDecoder with
                 | Ok projects -> projects
                 | Error _ -> { Forecast.Root.projects = [] }
    Assert.Equal(expected, actual)
