---
title: Use cases
category: Design
---

## Consistency checking

When I start the board solve, the current state of the project plan is typically
invalid in one of several ways. Here are a set of claims which should be true
but typically are not. 

1. Every Project in Forecast has a GitHub issue number in the “code” field, of
   the form `hut23-nnn`.
   - Except there is a single, hard-coded project about time off or something.

2. Each Forecast Project should exist on GitHub as an issue;
   - and all these issues should be in the “Project Tracker”
   
3. Each GitHub issue in 
   - Active;
   - Awaiting start; or
   - Finding people
   must be a Project on Forecast.
   
4. Allocations of Forecast should have a Finance code (or the Project has a
   single Finance code).

5. Projects that are Active or Awaiting start should have allocations (past and
   future) whose total resource is close to, but does not exceed, the resource
   requirements.
   
6. The total allocation to a Finance code, in a Forecast project, past and
   future, is “about equal” to the total resource required and is anywy not
   greater than it. “About equal” means “within 1 FTE-week.”.
   
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
