module NowWhat.API.Forecast

open FSharp.Data
open HttpFs.Client
open Hopac

let [<Literal>] ForecastUrl = "https://api.forecastapp.com/"

let forecastRequest (accountId: string) (forecastToken: string) endpoint =
  Request.createUrl Get (ForecastUrl + endpoint)
  |> Request.setHeader (Authorization ("Bearer " + forecastToken))
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
let getPeople (accountId: string) (forecastToken: string) =
  forecastRequest accountId forecastToken "people" |> People.Parse
let getAssignments (accountId: string) (forecastToken: string) =
  forecastRequest accountId forecastToken "assignments" |> Assignments.Parse
let getClients (accountId: string) (forecastToken: string) =
  forecastRequest accountId forecastToken "clients" |> Clients.Parse
let getPlaceholders (accountId: string) (forecastToken: string) =
  forecastRequest accountId forecastToken "placeholders" |> Placeholders.Parse
let getProjects (accountId: string) (forecastToken: string) =
  forecastRequest accountId forecastToken "projects" |> Projects.Parse
