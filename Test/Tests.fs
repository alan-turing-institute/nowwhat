module Tests

open System
open Xunit
open NowWhat.CLI

[<Fact>]
let ``My test`` () =
    nowwhat ()
    Assert.True(true)
