---
title: Use cases
category: Design
---

## Consistency checking

When I start the board solve, the current state of the project plan is typically
invalid in one of several ways. Some of those ways are critical to nowwhat being
able to proceed; others are merely warnings. 

### Panics

The system should panic, emit panic messages, and stop if any of these are not true.

1. Every Project in Forecast has a GitHub issue number in the “code” field, of
   the form `hut23-nnn`.
   - Except there is a single, hard-coded project "Time Off".

2. Each Forecast Project should exist on GitHub as an issue;
   - and all these issues should be in the “Project Tracker”
   
### Errors 

Errors are inconcistencies in our schedule that should be addressed as soon as
possible. 
   
1. Each GitHub issue in 
   - Active;
   - Awaiting start; or
   - Finding people
   must be a Project on Forecast.
   
2. Each project on Forecast should have a single Finance code (in the "Project
   Tags" field).

3. Each GitHub issue should have a complete set of YAML metadata.

### Warnings

Warngins are informative messages that someone will need to look at (eg, when
doing the board solve).

1. Projects that are Active or Awaiting start should have Finance code
   allocations (past and future) whose total resource is close to, but does not
   exceed, the resource requirements.
   
## Convenience reporting

I would like to be able to print a little table of the schedule, by week
(grouped by month), for the following groups:
- People
- Projects
- Assignments

and the following subgroups:
- All people who report to person X
- All assignments allocated to person X
- All projects led by person X (for whom X is the senior REG person)
- All projects in the Programme for which Person X is the Programme Lead



## What do I need to know about Projects to do the solve?

1. For a given Finance code, date and resource constraints:
   - Earliest start date
   - Latest start date
   - Latest end date
   - Total resource required, FTE-weeks (or maybe FTE-months)
   - Max, min, and nominal FTE run-rate
   
2. 
