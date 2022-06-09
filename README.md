# Now What?
## REG project tracking tool

Reimagination of [whatnow](https://github.com/alan-turing-institute/whatnow), also borrowing from [whatnext](https://github.com/alan-turing-institute/whatnext).

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet/5.0)

## Setup

### Git config

Run [`script/dev-setup.sh`](script/dev-setup.sh) from project root.

### Environment variables

Run the tool with the following environment variables set:

- `GITHUBID` - your Github ID
- `GITHUBTOKEN` - Github [personal access token for the application](https://docs.github.com/en/github/authenticating-to-github/creating-a-personal-access-token)
- `FORECASTID` - most easily found by logging in via web interface and
reading number that appears in URL just after server name
- `FORECASTTOKEN` - to obtain an authentication token log in to @emph{Harvest} and look for
the ``Developers'' section, where there will be an option to obtain a Personal Access Token

### Building, running and testing

To build or run, in the `NowWhat` folder run:

```
dotnet build
dotnet run
```

To test, in the `Test` folder run:
```
dotnet test
```
