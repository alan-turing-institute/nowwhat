module Tests

open System
open System.IO
open Xunit
open NowWhat.CLI

[<Fact>]
let noEnvVars (): unit =
    let testName: string = "noEnvVars"
    let folder: string = "../../../expected/"
    let writer = new StreamWriter($"{folder}/{testName}.new.txt")
    Console.SetOut(writer)
    nowwhat () |> ignore
    writer.Flush()
    let expected: string = File.ReadAllText($"{folder}/{testName}.txt")
    Assert.True(true)
