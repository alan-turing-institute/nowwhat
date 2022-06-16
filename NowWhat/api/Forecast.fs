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
    { id: int
      harvestId: int option
      clientId: int option
      name: string
      code: string option
      tags: string list
      notes: string option
      isArchived: bool }

// Can we elide this?
type Root = { projects: Project List }

(* ---------------------------------------------------------------------------------------------------
   Get Forecast objects as F# types
   *)

let projectDecoder: Decoder<Project> =
    Decode.object (fun get ->
        { Project.id = get.Required.Field "id" Decode.int
          Project.harvestId = get.Optional.Field "harvest_id" Decode.int
          Project.clientId = get.Optional.Field "client_id" Decode.int
          Project.name = get.Required.Field "name" Decode.string
          Project.code = get.Optional.Field "code" Decode.string
          Project.tags = get.Required.Field "tags" (Decode.list Decode.string)
          Project.notes = get.Optional.Field "notes" Decode.string
          Project.isArchived = get.Required.Field "archived" Decode.bool })

let rootDecoder: Decoder<Root> =
    Decode.object (fun get -> { Root.projects = get.Required.Field "projects" (Decode.list projectDecoder) })

(* -----------------------------------------------------------------------------
   Interface to the Forecast API

*)

[<Literal>]
let ForecastUrl = "https://api.forecastapp.com/"

let forecastRequest (endpoint: string) : string =

    let secrets =
        match getConfig () with
        | Ok secrets -> secrets
        | Error err -> raise (FailedException($"Forecast secrets could not be loaded. {err.ToString()}"))

    let response =
        Request.createUrl Get (ForecastUrl + endpoint)
        |> Request.setHeader (Authorization("Bearer " + secrets.forecastToken))
        |> Request.setHeader (Custom("Forecast-Account-ID", secrets.forecastId))
        |> getResponse
        |> run

    let responseBody = response |> Response.readBodyAsString |> run

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

    responseBody

(* -----------------------------------------------------------------------------
   Public interface to this module
*)

// Other useful endpoints are: people; assignments; clients; placeholders.

let getProjects () : Project list =
    match forecastRequest "projects"
          |> Decode.fromString rootDecoder
        with
    | Ok root -> root.projects
    | Error _ -> failwith "Unable to deserialise Forecast response."
