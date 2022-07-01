module NowWhat.Schedule

(*  Interface to domain data

    Obtains a complete schedule by unifying data from GitHub and Forecast

    Along the way, emit:
    - Panics (and stop) for things that prevent further processing;
    - Errors: for serious problems with the database as a whole
    - Warnings: for other issues
    - Message: for informative messages

    *)
    

open NowWhat.DomainModel
open NowWhat.API


(* -----------------------------------------------------------------------------
   Public interface
*)

/// Return a coherent, validated schedule
let getTheSchedule () : Schedule =
    { projects = []
  }
