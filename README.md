# Now What?
## REG project tracking tool

Reimagination of [whatnow](https://github.com/alan-turing-institute/whatnow), also borrowing from [whatnext](https://github.com/alan-turing-institute/whatnext).

## Prerequisites

- [.NET Core SDK 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
- [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/3.1/runtime?cid=getdotnetcore)

## Setup

### Git config

Run [`script/dev-setup.sh`](script/dev-setup.sh) from project root.

### Environment variables

Run the tool with the following environment variables set. [How to store sensitive environment variables on MacOS](https://medium.com/@johnjjung/how-to-store-sensitive-environment-variables-on-macos-76bd5ba464f6) may be useful.

- `NOWWHAT_GITHUB_TOKEN` - Github [personal access token](https://docs.github.com/en/github/authenticating-to-github/creating-a-personal-access-token)
- `FORECAST_ID` - most easily found by logging in via web interface and
reading number that appears in URL just after server name
- `NOWWHAT_FORECAST_TOKEN` - log into Forecast, go to My Profile and then Developers section; there will be an option to obtain a Personal Access Token

### Building and running

To build or run, in the `NowWhat` folder run:

```
dotnet build
dotnet run
```

### Testing

See [testing guidelines](Test/README.md).
