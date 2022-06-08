module NowWhat.API.Forecast

open FSharp.Data
open HttpFs.Client
open Hopac

let [<Literal>] HarvestUrl = "https://api.harvestapp.com/api/v2/"
let [<Literal>] ForecastUrl = "https://api.forecastapp.com/"

//let accountId = "968445" // for Harvest
let accountId = System.Environment.GetEnvironmentVariable "FORECASTID"
let personalAccessToken = System.Environment.GetEnvironmentVariable "FORECASTTOKEN"

let forecastRequest endpoint =
  Request.createUrl Get (ForecastUrl + endpoint)
  |> Request.setHeader (Authorization ("Bearer " + personalAccessToken))
  |> Request.setHeader (Custom ("Forecast-Account-ID", accountId))
  |> Request.responseAsString // UTF8-encoded
  |> run

let whoami () = forecastRequest "whoami"

type People = JsonProvider<"api/sample-json/forecast-people.json">
type Assignments = JsonProvider<"api/sample-json/forecast-assignments.json">
type Clients = JsonProvider<"api/sample-json/forecast-clients.json">
type Placeholders = JsonProvider<"api/sample-json/forecast-placeholders.json">
type Projects = JsonProvider<"api/sample-json/forecast-projects.json">

// Forecast endpoints
let getPeople () = forecastRequest "people" |> People.Parse
let getAssignments () = forecastRequest "assignments" |> Assignments.Parse
let getClients () = forecastRequest "clients" |> Clients.Parse
let getPlaceholders () = forecastRequest "placeholders" |> Placeholders.Parse
let getProjects () = forecastRequest "projects" |> Projects.Parse



