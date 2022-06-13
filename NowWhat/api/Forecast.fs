module NowWhat.API.Forecast

open FSharp.Data
open HttpFs.Client
open Hopac

// exception UnauthorisedException of System.Exception
exception HttpException of string

let [<Literal>] ForecastUrl = "https://api.forecastapp.com/"

let envVars = {|
  ForecastId = "FORECAST_ID";
  ForecastToken = "NOWWHAT_FORECAST_TOKEN"
|}

type People = JsonProvider<"api/sample-json/forecast-people.json">
type Assignments = JsonProvider<"api/sample-json/forecast-assignments.json">
type Clients = JsonProvider<"api/sample-json/forecast-clients.json">
type Placeholders = JsonProvider<"api/sample-json/forecast-placeholders.json">
type Projects = JsonProvider<"api/sample-json/forecast-projects.json">

let forecastRequest (endpoint: string) =
  let forecastId = System.Environment.GetEnvironmentVariable(envVars.ForecastId)
  let forecastToken = System.Environment.GetEnvironmentVariable(envVars.ForecastToken)
  let response = Request.createUrl Get (ForecastUrl + endpoint)
              |> Request.setHeader (Authorization ("Bearer " + forecastToken))
              |> Request.setHeader (Custom ("Forecast-Account-ID", forecastId))
              |> getResponse
              |> run
  let responseBody = response |> Response.readBodyAsString |> run
  if (response.statusCode < 200) || (response.statusCode > 299) then
    raise (HttpException $"Forecast request failed. Status code: {response.statusCode}; Message: {responseBody}")
  responseBody

// Forecast endpoint functions
let getPeople () =
  forecastRequest  "people" |> People.Parse

let getAssignments () =
  forecastRequest "assignments" |> Assignments.Parse

let getClients () =
  forecastRequest "clients" |> Clients.Parse

let getPlaceholders () =
  forecastRequest "placeholders" |> Placeholders.Parse

let getProjects () =
  forecastRequest "projects" |> Projects.Parse
