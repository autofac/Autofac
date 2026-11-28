---
name: issues
description: >-
  How to write a bug report or feature request for an Autofac repository
  that a maintainer can act on.
applyTo: "**"
---

# Issues

An issue is a request for a maintainer's time. Make it short, specific, and reproducible.

- File it in the repository that owns the code. Usage questions go to [Stack Overflow with the `autofac` tag](https://stackoverflow.com/questions/tagged/autofac), not to issues.
- Search existing issues, open and closed, before filing. Link a related one rather than repeating it.
- Follow the repository's issue template, and keep each section to what a maintainer needs.

## Bug Reports

- Give the smallest reproduction you can, ideally a failing xUnit test that uses only Autofac and the integration in question.
- Paste the full exception and stack trace as text, not a screenshot.
- List the versions of Autofac, the integration, and the .NET runtime.
- State what you expected and what happened, in a sentence each.

## Feature Requests

- Describe the problem you are trying to solve before the solution you want. A maintainer may know an existing way to do it.
- Show the API you have in mind with a short code sample.
- Say who else would benefit. Features are judged on how they serve Autofac users broadly and what they cost to maintain, following the library design rules.

Do not paste an AI-generated analysis of the codebase. If an AI tool helped you find the problem, confirm the problem yourself and report what you confirmed.
