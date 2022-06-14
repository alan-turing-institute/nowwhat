module Tests

open System
open System.IO
open Thoth.Json.Net
open Xunit
open NowWhat.CLI
open NowWhat.API
open NowWhat.DomainModel.Forecast

let redirectStdOut () =
    let stringWriter = new StringWriter()
    Console.SetOut(stringWriter)
    stringWriter

let readExpected (filename: string) =
    File.ReadAllText($"{__SOURCE_DIRECTORY__}/expected/{filename}")

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

[<Fact>]
let ``End-to-end test without environment variables`` () =
    let stdout = redirectStdOut ()
    let testfn () =
        let forecastId = Environment.GetEnvironmentVariable("FORECAST_ID")
        let forecastToken = Environment.GetEnvironmentVariable("NOWWHAT_FORECAST_TOKEN")
        for envVar in
            [ "FORECAST_ID"
              "NOWWHAT_FORECAST_TOKEN" ] do
            Environment.SetEnvironmentVariable(envVar, "")
        nowwhat () |> ignore
        Environment.SetEnvironmentVariable("FORECAST_ID", forecastId)
        Environment.SetEnvironmentVariable("NOWWHAT_FORECAST_TOKEN", forecastToken)

    testfn()
    // Assert.Throws<Forecast.HttpException>(testfn) |> ignore
    Assert.Equal(readExpected("noEnvVars.txt"), stdout.ToString())

[<Fact>]
let ``End-to-end test with environment variables`` () =
    let stdout = redirectStdOut ()
    nowwhat () |> ignore
    Assert.Equal(readExpected("withEnvVars.txt"), stdout.ToString())

[<Fact>]
let ``test Forecast JSON deserialisation`` () =
    match rootJson |> Decode.fromString rootDecoder with
    | Ok projects -> printfn $"Root: {projects}"
    | Error err ->
        printfn $"Error: {err}"
        Assert.True(false)
