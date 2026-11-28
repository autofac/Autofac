---
name: project-context
description: >-
  Orients an agent in an Autofac repository: what to read first, how the
  repositories relate, and the workflow every change follows.
applyTo: "**"
---

# Autofac Project Context

Autofac is a dependency injection container for .NET with a history going back to 2007 and many millions of downloads. Most of its code was written by people, and people maintain it. Contributions are judged on how well they serve the users and the maintainers years from now, not on how much they change.

## Read Before Working

- `README.md` in the repository, and the [contributor guide](https://github.com/autofac/.github/blob/main/CONTRIBUTING.md) shared by every Autofac repository. Where a repository has its own `CONTRIBUTING.md`, it adds to the shared guide.
- `.editorconfig`, `Directory.Build.props`, and the rulesets and `stylecop.json` under `build/`. Analyzers enforce these, and Release builds treat warnings as errors.
- `global.json` for the SDK, and `default.proj` for how the build runs. `dotnet msbuild ./default.proj` builds, tests, and packages most repositories.

## How the Repositories Fit Together

`autofac/Autofac` is the core container. Every other `Autofac.*` repository is an integration that depends on it, and some depend on each other, such as `Autofac.AspNetCore.Multitenant` on `Autofac.Multitenant`. A change to core can break integrations you are not looking at, and a change to an integration may need matching updates in [Documentation](https://github.com/autofac/Documentation) or [Examples](https://github.com/autofac/Examples).

File issues and pull requests in the repository that holds the code. A problem with the ASP.NET Core integration belongs in that repository, not in core.

## Workflow

- Libraries use Gitflow. Branch from `develop` and open pull requests against `develop`. Releases ship from `main`, and contributors never target it.
- Documentation, Examples, and this `.github` repository have no `develop` branch. Work against `main`.
- Behavior changes and new features start with an issue, so the design is agreed on before anyone writes code.
- Everything builds and tests with zero errors and zero warnings.
