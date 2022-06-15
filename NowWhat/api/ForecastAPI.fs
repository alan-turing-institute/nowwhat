module NowWhat.API.ForecastAPI

open HttpFs.Client
open Hopac
open Thoth.Json.Net
open NowWhat.Config
open NowWhat.DomainModel

exception FailedException of string
exception UnauthorisedException of string

let [<Literal>] ForecastUrl = "https://api.forecastapp.com/"

let forecastRequest (endpoint: string): string =
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

// Other useful endpoints are: people; assignments; clients; placeholders.

let getProjects (): ForecastModel.Project List =
  match forecastRequest "projects" |> Decode.fromString ForecastModel.rootDecoder with
  | Ok root -> root.projects
  | Error _ -> failwith "Unable to deserialise Forecast response."
