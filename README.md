# Now What?
## REG project tracking tool

Reimagination of [whatnow](https://github.com/alan-turing-institute/whatnow), also borrowing from [whatnext](https://github.com/alan-turing-institute/whatnext).

## Prerequisites

- [.NET Core 6.0 Runtime and SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
  - For Mac OSX you can run `brew install dotnet` (Runtime) and `brew install dotnet-sdk` (SDK)
  - To test your install is working runt `dotnet fsi` from the terminal (to quit run `#quit;;`)

Run `dotnet tool restore` and then `dotnet paket restore`.

## Setup

### Git config

Run [`script/dev-setup.sh`](script/dev-setup.sh) from project root.

### App configuration

The app needs the following information to authenticate to the GitHub and Forecast APIs.
- A Github [personal access token](https://docs.github.com/en/github/authenticating-to-github/creating-a-personal-access-token)
    - The scope you need is `repo` (`Full control of private repositories`)
- The Forecast ID for your organisation.
  - Most easily found by logging into [Forecast](https://forecastapp.com/) via web interface and reading number that appears in URL just after server name
- A Forecast personal access token.
  - Log into [Forecast](https://forecastapp.com/), go to My Profile and then Developers section; there will be an option to obtain a Personal Access Token

#### Set via config file
- Create a file in `~/.config/nowwhat/secrets.json` and add each variable as a top-level JSON field. e.g.

```json
{
  "gitHubToken": "<NOWWHAT_GITHUB_TOKEN>",
  "forecastId": "<FORECAST_ID>",
  "forecastToken": "<NOWWHAT_FORECAST_TOKEN>"
}
```

#### Set via environment variables
[How to store sensitive environment variables on MacOS](https://medium.com/@johnjjung/how-to-store-sensitive-environment-variables-on-macos-76bd5ba464f6) may be useful.

- `NOWWHAT_GITHUB_TOKEN` - GitHub personal access token
- `FORECAST_ID` - Forecast organisation ID
- `NOWWHAT_FORECAST_TOKEN` - Forecast personal access token

### Building and running

To build or run, in the `NowWhat` folder run:

```
dotnet build
dotnet run
```

### Testing

See [testing guidelines](Test/README.md).

### Writing documentation

See 
- [`FSharp.Formatting`](https://fsprojects.github.io/FSharp.Formatting/) for the documentation system
- [XML documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/xml-documentation) for writing docs
