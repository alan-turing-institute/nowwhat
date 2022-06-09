module Tests

open System
open System.IO
open Xunit
open NowWhat.CLI

[<Fact>]
let noEnvVars (): unit =
    let testName: string = "noEnvVars"
    let folder: string = "../../../expected/"
    let ext: string = "txt"
    let foundFile: string = $"{folder}/{testName}.new.{ext}"
    let writer = new StreamWriter(foundFile)
    Console.SetOut(writer)
    nowwhat () |> ignore
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
    Assert.True(equal)
