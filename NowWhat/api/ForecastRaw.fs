module NowWhat.API.ForecastRaw

open HttpFs.Client
open Hopac
open Thoth.Json.Net

open NowWhat.Config

exception FailedException of string
exception UnauthorisedException of string

(* -----------------------------------------------------------------------------
   Forecast data model

   This model describes objects as Forecast thinks of them. We impose a further
   set of constraints on this model in order to use Forecast objects to
   represent entities as we want to think of them.

*)

// Interface to Forecast API

[<Literal>]
let ForecastUrl = "https://api.forecastapp.com/"

let forecastRequest (endpoint: string) : string =

    let secrets = getConfig () 

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



// ------------------------------------------------------------
// Projects

type Project =
    { Id: int
      HarvestId:  int option
      ClientId:   int option
      Name:       string
      Code:       string option
      Tags:       string list
      Notes:      string option
      IsArchived: bool }

let projectDecoder : Decoder<Project> =
    Decode.object (fun get ->
        { Project.Id         = get.Required.Field "id"         Decode.int
          Project.HarvestId  = get.Optional.Field "harvest_id" Decode.int
          Project.ClientId   = get.Optional.Field "client_id"  Decode.int
          Project.Name       = get.Required.Field "name"       Decode.string
          Project.Code       = get.Optional.Field "code"       Decode.string
          Project.Tags       = get.Required.Field "tags"       (Decode.list Decode.string)
          Project.Notes      = get.Optional.Field "notes"      Decode.string
          Project.IsArchived = get.Required.Field "archived"   Decode.bool })

let projectsDecoder : Decoder<Project list> =
    Decode.object (fun get -> get.Required.Field "projects" (Decode.list projectDecoder) )

let getProjects () : Project list =
    match forecastRequest "projects"
          |> Decode.fromString projectsDecoder
        with
    | Ok root -> root
    | Error _ -> failwith "Unable to deserialise Forecast response (for projects)."


// ------------------------------------------------------------
// Clients

type Client =
    { Id:         int
      Name:       string
      IsArchived: bool }

let clientDecoder : Decoder<Client> =
    Decode.object (fun get ->
        { Client.Id         = get.Required.Field "id"       Decode.int
          Client.Name       = get.Required.Field "name"     Decode.string
          Client.IsArchived = get.Required.Field "archived" Decode.bool})

let clientsDecoder : Decoder<Client list> =
    Decode.object (fun get -> get.Required.Field "clients" (Decode.list clientDecoder) )

let getClients () : Client list =
    match forecastRequest "clients"
          |> Decode.fromString clientsDecoder
      with
    | Ok root -> root
    | Error _ -> failwith "Unable to deserialise Forecast response (for clients)."


// ------------------------------------------------------------
// People

type Person =
    { Id:         int
      HarvestId:  int option
      FirstName:  string
      LastName:   string
      Email:      string option
      MayLogin:   string
      Roles:      string list
      IsArchived: bool }

let personDecoder : Decoder<Person> =
    Decode.object (fun get ->
        { Person.Id         = get.Required.Field "id"         Decode.int
          Person.HarvestId  = get.Optional.Field "harvest_id" Decode.int
          Person.FirstName  = get.Required.Field "first_name" Decode.string
          Person.LastName   = get.Required.Field "last_name"  Decode.string
          Person.Email      = get.Optional.Field "email"      Decode.string
          Person.MayLogin   = get.Required.Field "login"      Decode.string
          Person.Roles      = get.Required.Field "roles"      (Decode.list Decode.string)
          Person.IsArchived = get.Required.Field "archived"   Decode.bool })

let peopleDecoder : Decoder<Person list> =
    Decode.object (fun get -> get.Required.Field "people" (Decode.list personDecoder) )

let getPeople () : Person list =
    match forecastRequest "people"
          |> Decode.fromString peopleDecoder
      with
    | Ok root -> root
    | Error _ -> failwith "Unable to deserialise Forecast response (for people)."
    

// ------------------------------------------------------------
// Placeholders

type Placeholder =
    { Id:         int
      Name:       string
      Roles:      string list
      IsArchived: bool }

let placeholderDecoder : Decoder<Placeholder> =
    Decode.object (fun get ->
        { Id         = get.Required.Field "id"       Decode.int
          Name       = get.Required.Field "name"     Decode.string
          Roles      = get.Required.Field "roles"    (Decode.list Decode.string)
          IsArchived = get.Required.Field "archived" Decode.bool })        

let placeholdersDecoder : Decoder<Placeholder list> =
    Decode.object (fun get -> get.Required.Field "placeholders" (Decode.list placeholderDecoder)) 


let getPlaceholders () : Placeholder list =
    match forecastRequest "placeholders"
          |> Decode.fromString placeholdersDecoder
      with
    | Ok root -> root
    | Error _ -> failwith "Unable to deserialise Forecast response (for placeholders)."

    
// ------------------------------------------------------------
// Assignments

type Assignment =
    { Id:            int
      ProjectId:     int
      // If PersonID is None, this is an Assignment to a Placeholder
      PersonId:      int option 
      PlaceholderId: int option
      StartDate:     string
      EndDate:       string
      Allocation:    int
      Notes:         string option }
      
let assignmentDecoder : Decoder<Assignment> =
    Decode.object (fun get ->
        { Assignment.Id            = get.Required.Field "id"         Decode.int
          Assignment.ProjectId     = get.Required.Field "project_id" Decode.int
          Assignment.PersonId      = get.Optional.Field "person_id"  Decode.int
          Assignment.PlaceholderId = get.Optional.Field "placeholder_id" Decode.int
          Assignment.StartDate     = get.Required.Field "start_date" Decode.string
          Assignment.EndDate       = get.Required.Field "end_date"   Decode.string
          Assignment.Allocation    = get.Required.Field "allocation" Decode.int
          Assignment.Notes         = get.Optional.Field "notes"      Decode.string })

let assignmentsDecoder : Decoder<Assignment list> =
    Decode.object (fun get -> get.Required.Field "assignments" (Decode.list assignmentDecoder))


// The Forecast API requires a start and end date for assingments.
// All assingments which overlap these dates are returned.
// The end date may not be more than 180 days after start date

    
let getAssignments startDate endDate : Assignment list =
    match forecastRequest "assignments"
          |> Decode.fromString assignmentsDecoder
      with
    | Ok root -> root
    | Error _ -> failwith "Unable to deserialise Forecast response (for assignments)."
    
