module NowWhat.CLI

open NowWhat.API

let nowwhat argv =
  try
    printfn "Now what?"

    let forecastProjects = Forecast.getProjects()
    printfn $"Number of projects in Forecast: {(forecastProjects.Projects |> Seq.length)}"

    0
  with
    | Forecast.UnauthorisedException(string) -> 
      printfn $"ERROR: {string}"
      -1
    | _ -> -2

