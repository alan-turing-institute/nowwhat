module NowWhat.DomainModel.Forecast

open Thoth.Json.Net

exception JsonDecodeException of string

type Project = {
  id: int;
  name: string;
  color: string;
  code: string option;
  notes: string option;
}

type Root = {
  projects: Project List
}

let projectDecoder : Decoder<Project> =
    Decode.object (
        fun get -> {
            Project.id = get.Required.Field "id" Decode.int;
            Project.name = get.Required.Field "name" Decode.string;
            Project.color = get.Required.Field "color" Decode.string;
            Project.code = get.Optional.Field "code" Decode.string;
            Project.notes = get.Optional.Field "notes" Decode.string;
        }
    )

let rootDecoder : Decoder<Root> =
    Decode.object (
      fun get -> {
        Root.projects = get.Required.Field "projects" (Decode.list projectDecoder)
      }
    )