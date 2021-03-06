# Now What?
## REG project tracking tool

Reimagination of [whatnow](https://github.com/alan-turing-institute/whatnow).

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet/5.0)

## Setup
### Environment variables

Create the following environment variables before running the tool:

- `GITHUBID` - your Github ID
- `GITHUBTOKEN` - Github [personal access token for the application](https://docs.github.com/en/github/authenticating-to-github/creating-a-personal-access-token)
- `FORECASTID` - 
- `FORECASTTOKEN` - 
The account id is most easily found by logging in via the web interface and
reading the number that appears in the URL just after the server name. To obtain
an authentication token you will need to log in to @emph{Harvest} and look for
the ``Developers'' section, within which there will be an option to obtain a
``Personal Access Token.'

### Build

```
dotnet build
```

## Running the tool

```
dotnet run
```

