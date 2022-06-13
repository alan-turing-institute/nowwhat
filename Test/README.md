# Testing guidelines

A lightly opinionated guide to testing for `nowwhat`.

## Running the tests

In the `Test` folder run:
```
dotnet test
```
We use the [xUnit](https://github.com/xunit/xunit) test framework for .NET.

### Pre-commit hook

The [dev setup script](../script/dev-setup.sh) will install a pre-commit hook that runs the tests on every commit. If you haven't used pre-commit hooks in this way before, we recommend giving it a try; it encourages a discipline where commits represent "good states".

## GitHub and Forecast sandboxes

Tests need to be deterministic, so it is best if they don't run on live data.
- GitHub: tests run against the [test repo](https://github.com/alan-turing-institute/Hut23-test).
- Forecast: tests currently run against the live system (see [#21](/../../issues/21)).

## Git aliases

- `git doc`
- `git refactor`
- `git rename` (a variant of `git refactor`)
