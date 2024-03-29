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
    { GithubToken   : string
      ForecastId    : string
      ForecastToken : string
  }

let loadConfig () : Config =
    let homeDir = System.Environment.GetFolderPath System.Environment.SpecialFolder.UserProfile
    let pathToConfig = homeDir + "/" + ".config/nowwhat/secrets.json"
    if not (File.Exists pathToConfig) then
      raise (LoadException $"Secrets file not found at '{pathToConfig}'")
    else
      let maybeConfig = Decode.Auto.fromString<Config>(File.ReadAllText pathToConfig, CaseStrategy.CamelCase)
      match maybeConfig with
        | Ok config -> config
        | Error err -> raise (LoadException err)

/// Look up secrets for connection details. First look in the enivronment
/// variables; then, if any cannot be found, from a config file in
/// $HOME/.config/nowwhat/secrets.json
let lazyConfig =
    lazy (
        let config =
            { ForecastId = System.Environment.GetEnvironmentVariable("FORECAST_ID")
              ForecastToken = System.Environment.GetEnvironmentVariable("NOWWHAT_FORECAST_TOKEN")
              GithubToken = System.Environment.GetEnvironmentVariable("NOWWHAT_GITHUB_TOKEN")
          }

        // printfn $"config.forecastId: '{config.forecastId}'"
        // printfn $"config.forecastToken: '{config.forecastToken}'"
        // printfn $"config.githubToken: '{config.githubToken}'"
        if (config.ForecastId = null || config.ForecastToken = null || config.GithubToken = null) then
           loadConfig ()
        else
           config
    )

/// Return server credentials from either environment variables (if defined) or
/// a config file. The file will only be read once.
let getConfig () = 
      lazyConfig.Force()
      
