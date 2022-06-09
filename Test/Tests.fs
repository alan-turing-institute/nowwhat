module Tests

open System
open System.IO
open Xunit
open NowWhat.CLI

// look for a more Xunit-idiomatic way of doing this
let test (testName: string) (doTest: unit -> unit): unit =
    let folder: string = "../../../expected/"
    let ext: string = "txt"
    let foundFile: string = $"{folder}/{testName}.new.{ext}"
    let writer = new StreamWriter(foundFile)
    Console.SetOut(writer)
    doTest()
    writer.Flush()
    let expectedFile: string = $"{folder}/{testName}.{ext}"
    let expected: string = File.ReadAllText(expectedFile)
    let found: string = File.ReadAllText(foundFile)
    let equal: bool = expected = found
    if equal then
        printfn $"{testName}: passed."
        File.Delete(foundFile)
    else
        printfn $"{testName}: failed.\nFound:\n{found}\nExpected:\n{expected}"
    Assert.Equal(expected, found)

[<Fact>]
let noEnvVars (): unit =
    test "noEnvVars" (fun () ->
        for envVar in gitHubVars @ forecastVars do
            Environment.SetEnvironmentVariable(envVar, "")
        nowwhat () |> ignore
    )
