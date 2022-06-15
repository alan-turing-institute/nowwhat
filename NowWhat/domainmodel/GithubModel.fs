module NowWhat.DomainModel.GithubModel
open Thoth.Json.Net

type Project = {
  number: int;
  name: string;
}

type ProjectRoot = {
  projects: Project List
}

let projectDecoder : Decoder<Project> =
    Decode.object (
        fun get -> {
          Project.number = get.Required.At ["node"; "number"] Decode.int 
          Project.name = get.Required.At ["node"; "name"] Decode.string
        }
    )

let projectRootDecoder : Decoder<ProjectRoot> =
    Decode.object (
        fun get -> {
          ProjectRoot.projects = get.Required.At ["data"; "repository"; "projects"; "edges"] (Decode.list projectDecoder)
          
        }
    )

type Issue = {
    number: int;
    // reactions: Reaction List;
}

type IssueRoot = {
  issue: Issue;
}

let issueDecoder : Decoder<Issue> =
    Decode.object (
        fun get -> {
            Issue.number = get.Required.Field "number" Decode.int 
            // Issue.url= get.Required.Field "url" Decode.string;
        }
    )

let issueRootDecoder : Decoder<IssueRoot> =
  Decode.object (
    fun get -> {
      IssueRoot.issue = get.Required.At ["data"; "repository"; "issue"] issueDecoder
    }
  )
