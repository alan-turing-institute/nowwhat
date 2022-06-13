module NowWhat.CLI

open NowWhat.API

let envVars = {|
  forecastId = "FORECAST_ID";
  forecastToken = "NOWWHAT_FORECAST_TOKEN"
|}

let forecastVars = [envVars.forecastId; envVars.forecastToken]

let checkEnvironmentVariables (envVars: string List) =
  let env = System.Environment.GetEnvironmentVariables()

  (true, envVars)
  ||> List.fold (fun allExist variable ->
    if not (env.Contains variable) then
      printfn $"Please set environment variable {variable}."
      false
    else
      allExist)

let forecastCredentials (): Forecast.Session =
  let forecastId = System.Environment.GetEnvironmentVariable("FORECAST_ID")
  let forecastToken = System.Environment.GetEnvironmentVariable("NOWWHAT_FORECAST_TOKEN")
  {| accountId = forecastId; forecastToken = forecastToken; |}

let nowwhat argv =
    printfn "Now what?"

    if checkEnvironmentVariables forecastVars then
      let forecastId = System.Environment.GetEnvironmentVariable(envVars.forecastId)
      let forecastToken = System.Environment.GetEnvironmentVariable(envVars.forecastToken)
      let forecastProjects = Forecast.getProjects <| forecastCredentials()

      printfn "Number of projects in Forecast: %d" (forecastProjects.Projects |> Seq.length)

    0
