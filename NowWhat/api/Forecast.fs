module NowWhat.API.Forecast

open HttpFs.Client
open Hopac
open Thoth.Json.Net

open NowWhat.Config

exception FailedException of string
exception UnauthorisedException of string

(* ---------------------------------------------------------------------------------------------------

   Forecast data model

*)
type Project =
    { Id: int
      HarvestId: int option
      ClientId: int option
      Name: string
      Code: string option
      Tags: string list
      Notes: string option
      IsArchived: bool }

// Can we elide this?
type Root = { projects: Project List }

(* ---------------------------------------------------------------------------------------------------
   Get Forecast objects as F# types
   *)

let projectDecoder: Decoder<Project> =
    Decode.object (fun get ->
        { Project.Id = get.Required.Field "id" Decode.int
          Project.HarvestId = get.Optional.Field "harvest_id" Decode.int
          Project.ClientId = get.Optional.Field "client_id" Decode.int
          Project.Name = get.Required.Field "name" Decode.string
          Project.Code = get.Optional.Field "code" Decode.string
          Project.Tags = get.Required.Field "tags" (Decode.list Decode.string)
          Project.Notes = get.Optional.Field "notes" Decode.string
          Project.IsArchived = get.Required.Field "archived" Decode.bool })

let rootDecoder: Decoder<Root> =
    Decode.object (fun get -> { Root.projects = get.Required.Field "projects" (Decode.list projectDecoder) })

(* -----------------------------------------------------------------------------
   Interface to the Forecast API

*)

[<Literal>]
let ForecastUrl = "https://api.forecastapp.com/"

let forecastRequest (endpoint: string) = async {

    let secrets = getConfig () 

    let response =
        Request.createUrl Get (ForecastUrl + endpoint)
        |> Request.setHeader (Authorization("Bearer " + secrets.ForecastToken))
        |> Request.setHeader (Custom("Forecast-Account-ID", secrets.ForecastId))
        |> getResponse
        |> run

    let responseBody = 
      response 
      |> Response.readBodyAsString 
      |> run

    if (response.statusCode < 200)
       || (response.statusCode > 299) then
        match response.statusCode with
        | 401 ->
            raise (
                UnauthorisedException
                    $"Forecast API authorisation failed. Status code: {response.statusCode}; Message: {responseBody}"
            )
        | _ ->
            raise (
                FailedException $"Forecast request failed. Status code: {response.statusCode}; Message: {responseBody}"
            )

    return responseBody
}

(* -----------------------------------------------------------------------------
   Public interface to this module
*)

// Other useful endpoints are: people; assignments; clients; placeholders.

let getProjects ()  = async {
    let! forecastResult = forecastRequest "projects" 
  
    match forecastResult |> Decode.fromString rootDecoder with
    | Ok root -> return root.projects
    | Error _ -> 
      failwith "Unable to deserialise Forecast response."
      return []
}
