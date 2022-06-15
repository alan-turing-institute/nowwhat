module NowWhat.API.ForecastAPI
open NowWhat.Config

open FSharp.Data
open HttpFs.Client
open Hopac

exception FailedException of string
exception UnauthorisedException of string

let [<Literal>] ForecastUrl = "https://api.forecastapp.com/"


type People = JsonProvider<"api/sample-json/forecast-people.json">
type Assignments = JsonProvider<"api/sample-json/forecast-assignments.json">
type Clients = JsonProvider<"api/sample-json/forecast-clients.json">
type Placeholders = JsonProvider<"api/sample-json/forecast-placeholders.json">
type Projects = JsonProvider<"api/sample-json/forecast-projects.json">

let forecastRequest (endpoint: string) =
  let secrets = match getSecrets () with
                | Ok secrets -> secrets
                | Error err -> raise (FailedException("Forecast secrets could not be loaded. {err.ToString()}"))
  let response = Request.createUrl Get (ForecastUrl + endpoint)
              |> Request.setHeader (Authorization ("Bearer " + secrets.forecastToken))
              |> Request.setHeader (Custom ("Forecast-Account-ID", secrets.forecastId))
              |> getResponse
              |> run
  let responseBody = response |> Response.readBodyAsString |> run
  if (response.statusCode < 200) || (response.statusCode > 299) then
    match response.statusCode with
      | 401 -> raise (UnauthorisedException $"Forecast API authorisation failed. Status code: {response.statusCode}; Message: {responseBody}")
      | _ -> raise (FailedException $"Forecast request failed. Status code: {response.statusCode}; Message: {responseBody}")
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
