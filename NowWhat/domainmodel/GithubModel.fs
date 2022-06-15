module NowWhat.DomainModel.GithubModel
open Thoth.Json.Net

// type User = {
//     login: string;
//     name: string option;
// }

// type Reaction = {
//     content:string option;
//     users: User List;
// }

type Issue = {
    id: int;
    // reactions: Reaction List;
}

type Root = {
  issue: Issue;
}

let issueDecoder : Decoder<Issue> =
    Decode.object (
        fun get -> {
            Issue.id = get.Required.Field "number" Decode.int 
            // Issue.url= get.Required.Field "url" Decode.string;
        }
    )

let rootDecoder : Decoder<Root> =
  Decode.object (
    fun get -> {
      Root.issue = get.Required.At ["data"; "repository"; "issue"] issueDecoder
    }
  )
