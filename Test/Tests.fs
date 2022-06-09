module Tests

open System
open System.IO
open Xunit
open NowWhat.CLI

[<Fact>]
let noEnvVars () =
    printfn $"Current directory {Environment.CurrentDirectory}"
    let writer = new StreamWriter("../../../expected/noEnvVars.new.txt")
    Console.SetOut(writer)
    nowwhat () |> ignore
    writer.Flush()
    Assert.True(true)
