module NowWhat.Config

(*
       Obtain user-specific configuration data, including:
       - The user's Forecast ID and Personal Access Token
       - The user's GitHub Personal Access Token

       Looks first in $HOME/.config/nowwhat/secrets.json, then in the environment
       variables NOWWHAT_GITHUB_TOKEN; FORECAST_ID; and NOWWHAT_FORECAST_TOKEN
*)

open Thoth.Json.Net
open System.IO

exception LoadException of string

type Config =
    { githubToken   : string
      forecastId    : string
      forecastToken : string
  }

let loadConfig () : Config =
    let homeDir = System.Environment.GetFolderPath System.Environment.SpecialFolder.UserProfile
    let pathToConfig = homeDir + "/" + ".config/nowwhat/secrets.json"
    if not (File.Exists pathToConfig) then
      raise (LoadException $"Secrets file not found at '{pathToConfig}'")
    else

    let maybeConfig = Decode.Auto.fromString<Config>(File.ReadAllText pathToConfig)

    match maybeConfig with
        | Ok config -> config
        | Error err -> raise (LoadException $"Could not parse secrets file: '{err}")


let overrideConfigVarFromEnvVarIfExists(configVal, envVarName) =
  let value = System.Environment.GetEnvironmentVariable(envVarName)
  if value <> null then value else configVal

/// Look up secrets for connection details. First look in the enivronment
/// variables; then, if any cannot be found, from a config file in
/// $HOME/.config/nowwhat/secrets.json
let lazyConfig =
    lazy (
        let config = loadConfig ()
        // Override config variables from environment if corresponding
        // environment variables are set
        { config with 
            forecastId = overrideConfigVarFromEnvVarIfExists(config.forecastId, "FORECAST_ID")
            forecastToken = overrideConfigVarFromEnvVarIfExists(config.forecastToken, "NOWWHAT_FORECAST_TOKEN")
            githubToken = overrideConfigVarFromEnvVarIfExists(config.githubToken, "NOWWHAT_GITHUB_TOKEN")
          }
    )


/// Return server credentials from either environment variables (if defined) or
/// a config file. The file will only be read once.
let getConfig (): Result<Config, exn> =
    try
      let config = lazyConfig.Force()
      Ok config
    with
    | e -> Error e
