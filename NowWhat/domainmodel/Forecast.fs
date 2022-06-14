module NowWhat.DomainModel.Forecast

open Thoth.Json.Net

type Project = {
  id: int
}

let projectDecoder : Decoder<Project> =
    Decode.object (
        fun get ->
            { Project.id = get.Required.Field "id" Decode.int }
    )
