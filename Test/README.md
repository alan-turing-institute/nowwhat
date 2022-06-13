# Testing guidelines

A lightly opinionated guide to testing for `nowwhat`.

## Running the tests

In the `Test` folder run:
```
dotnet test
```
We use the [xUnit](https://github.com/xunit/xunit) test framework for .NET.

## GitHub and Forecast sandboxes

Tests need to be deterministic, so it is best if they don't run on live data.
- GitHub: tests run against the [test repo](https://github.com/alan-turing-institute/Hut23-test).
- Forecast: tests currently run against the live system (but see [#22](/../../issues/22)).

### Pre-commit hook

The [dev setup](../script/dev-setup.sh) script will install a pre-commit hook that runs the tests on every commit. If you haven't used pre-commit hooks in this way before, we recommend giving it a try; it encourages a discipline where commits represent "good states". The idea is not to encourage you to commit less often, but rather to look for smaller commits that move you closer to your goal without breaking anything.

Currently the pre-commit hook will run the tests on any commit, but we could refine that to exclude documentation-only changes (see [#23](/../../issues/23)).

## Testing methodology

- store test "expectations" (expected outputs, baselines) in text files (rather than string literals in test cases)

### Git aliases

The following git aliases explicitly support the "approval testing" approach:

- `git test-approve`
- `git test-reject`
- `git test-compare`

The idea is to make it easy to compare/accept/reject changes to test baselines. For example, the (current) expected console output for running `nowwhat` with no environment variables set is something like:

>```
>Now what?
>Please set environment variable GITHUBID.
>Please set environment variable GITHUBTOKEN.
>Please set environment variable FORECASTID.
>Please set environment variable FORECASTTOKEN.

Suppose we no longer want `Now what?` to be printed out, so we make that change to the code. When we run the tests, we'll get a test failure:

<img width="672" alt="Screenshot 2022-06-09 at 09 55 59" src="https://user-images.githubusercontent.com/121074/172807697-73253c99-131d-479e-9299-f5386641c89f.png">

`git status` will reveal a `.new` test expectation:

<img width="672" alt="Screenshot 2022-06-09 at 09 57 43" src="https://user-images.githubusercontent.com/121074/172807908-18e83223-98d3-4af3-bb88-5d3bb7aa6c83.png">

It's a good idea not to overwrite existing expectation files, to make it harder to accidentially commit changes to the baselines. You can get a more readable diff with `git test-compare`:

<img width="672" alt="Screenshot 2022-06-09 at 09 53 41" src="https://user-images.githubusercontent.com/121074/172807074-78aa66b3-c907-46a1-869c-c7dee38b9b53.png">

and finally you can approve the change (overwrite the existing baseline) with `git test-approve`, which overwrites `expected/noEnvVars.txt` with `expected/noEnvVars.new.txt`, which can then be committed along with the code change.

So the idea is to treat behaviour as invariant by default and something that only changes by an explicit (intentional) action.

These approve, reject or compare test expectations with actual outputs.

We also provide the following aliases, which should be self-explanatory:

- `git doc`
- `git refactor`
- `git rename` (variant of `git refactor`)
