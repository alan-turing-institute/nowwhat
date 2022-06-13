module NowWhat.CLI

open NowWhat.API

let nowwhat argv =
    printfn "Now what?"

    let forecastProjects = Forecast.getProjects()
    printfn "Number of projects in Forecast: %d" (forecastProjects.Projects |> Seq.length)

    0
