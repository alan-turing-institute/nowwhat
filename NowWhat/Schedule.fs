module NowWhat.Schedule

(*  Interface to domain data

    Obtains a complete schedule by unifying data from GitHub and Forecast
    Along the way:
    - emits errors (and stops) for things that prevent further processing;
    - emits warnings for other problems

    *)

open NowWhat.API.Forecast
open NowWhat.API.Github

type Project = { id: int }

type Schedule = { projects: Project list }
