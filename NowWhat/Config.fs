module NowWhat.Config

(*
       Obtain user-specific configuration data, including:
       - The user's Forecast ID and Personal Access Token
       - The user's GitHub Personal Access Token

       Looks first in $HOME/.config/nowwhat/secrets.xml, then in the environment
       variables NOWWHAT_GITHUB_TOKEN; FORECAST_ID; and NOWWHAT_FORECAST_TOKEN
*)

open Thoth.Json.Net
open System.IO

let [<Literal>] secretsFile = __SOURCE_DIRECTORY__ + "/secrets.json"

exception SecretLoadException of string

type Secrets =
    { githubToken   : string
      forecastId    : string
      forecastToken : string
  }

type Config =
    { NOWWHAT_GITHUB_TOKEN   : string
      FORECAST_ID            : string
      NOWWHAT_FORECAST_TOKEN : string
  }

let getSecretsFromConfig () : Secrets =
    let homeDir = System.Environment.GetFolderPath System.Environment.SpecialFolder.UserProfile
    let pathToConfig = homeDir + "/" + ".config/nowwhat/secrets.json"
    let maybeSecrets = Decode.Auto.fromString<Config>(File.ReadAllText pathToConfig)

    match maybeSecrets with
        | Ok config -> { githubToken   = config.NOWWHAT_GITHUB_TOKEN
                         forecastId    = config.FORECAST_ID
                         forecastToken = config.NOWWHAT_FORECAST_TOKEN }
        | Error err -> raise (SecretLoadException err)

/// Look up secrets for connection details. First look in the enivronment
/// variables; then, if any cannot be found, from a config file in
/// $HOME/.config/nowwhat/secrets.json
let lazySecrets =
    lazy (
        let secrets =
            { forecastId = System.Environment.GetEnvironmentVariable("FORECAST_ID")
              forecastToken = System.Environment.GetEnvironmentVariable("NOWWHAT_FORECAST_TOKEN")
              githubToken = System.Environment.GetEnvironmentVariable("NOWWHAT_GITHUB_TOKEN")
          }

        // printfn $"secrets.forecastId: '{secrets.forecastId}'"
        // printfn $"secrets.forecastToken: '{secrets.forecastToken}'"
        // printfn $"secrets.githubToken: '{secrets.githubToken}'"
        if (secrets.forecastId = null || secrets.forecastToken = null || secrets.githubToken = null) then
           getSecretsFromConfig ()
        else
           secrets
    )

/// getSecrets only opens the config file once even if called multiple times
let getSecrets () =
    try
      let secrets = lazySecrets.Force()
      Ok secrets
    with
    | e -> Error e
