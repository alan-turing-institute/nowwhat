module Tests

open System
open System.IO
open Thoth.Json.Net
open Xunit
open NowWhat.CLI
open NowWhat.API
open NowWhat.DomainModel.Forecast

// TODO: more Xunit-idiomatic way of doing this?
let test (testName: string) (doTest: unit -> unit): unit =
    let folder: string = "../../../expected/"
    let ext: string = "txt"
    let foundFile: string = $"{folder}/{testName}.new.{ext}"

    let writer = new StreamWriter(foundFile)
    Console.SetOut(writer)
    let restoreDir = Directory.GetCurrentDirectory()
    Directory.SetCurrentDirectory("../../../../NowWhat")

    try
        doTest()
    finally
        Directory.SetCurrentDirectory(restoreDir)
        writer.Flush()
        let stdout = new StreamWriter(Console.OpenStandardOutput())
        Console.SetOut(stdout)

    let expectedFile: string = $"{folder}/{testName}.{ext}"
    let expected: string =
        if File.Exists(expectedFile) then File.ReadAllText(expectedFile) else ""
    let found: string = File.ReadAllText(foundFile)
    let equal: bool = expected = found
    if equal then
        printfn $"{testName}: passed."
        File.Delete(foundFile)
    else
        printfn $"{testName}: failed.\nFound:\n{found}\nExpected:\n{expected}"
    stdout.Flush()
    Assert.Equal(expected, found)

[<Fact>]
let test_noEnvVars (): unit =
    test "noEnvVars" (fun () ->
        // https://github.com/alan-turing-institute/nowwhat/issues/11
        let forecastId = Environment.GetEnvironmentVariable(Forecast.envVars.ForecastId)
        let forecastToken = Environment.GetEnvironmentVariable(Forecast.envVars.ForecastToken)
        for envVar in [Forecast.envVars.ForecastId; Forecast.envVars.ForecastToken] do
            Environment.SetEnvironmentVariable(envVar, "")
        nowwhat () |> ignore
        Environment.SetEnvironmentVariable(Forecast.envVars.ForecastId, forecastId)
        Environment.SetEnvironmentVariable(Forecast.envVars.ForecastToken, forecastToken)
    )

[<Fact>]
let test_withEnvVars (): unit =
    test "withEnvVars" (fun () ->
        nowwhat () |> ignore
    )

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
let test_Forecast_project_deserialise (): unit =
    match rootJson |> Decode.fromString rootDecoder with
    | Ok projects -> printfn $"Root: {projects}"
    | Error err ->
        printfn $"Error: {err}"
        Assert.True(false)
