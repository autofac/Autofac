---
name: pull-request
description: >-
  How to prepare, describe, and follow up on a pull request to an Autofac
  repository, including disclosure of AI assistance.
applyTo: "**"
---

# Pull Requests

## Before Opening One

- Start from an issue for any behavior change or new feature, and wait until a maintainer agrees on the approach. A pull request that arrives with no issue risks rejection on design alone, however good the code is.
- Target `develop` in library repositories, and `main` in repositories that have no `develop` branch.
- Keep the change to one purpose. Do not bundle reformatting, renames, package upgrades, or "while I was here" fixes. Each of those is its own pull request, if it is wanted at all.
- Include tests that fail without the change and pass with it.
- Build and run the tests locally, with zero errors and zero warnings, before asking anyone to review.
- Review your own diff first. Remove debugging code, unrelated edits, and generated files that do not belong.

## Title and Description

The title becomes the commit subject when the pull request is squashed, so it follows the commit message rules.

The description tells a maintainer what problem this solves, how, and how you know it works. A maintainer should be able to read it in about a minute.

- Link the issue with `Fixes #123` or `Refs #123`.
- Say what changed in behavior, not which files changed.
- Call out anything a reviewer must not miss: a public API addition, a breaking change, a new dependency, or a behavior change for existing users.
- For a performance change, give one benchmark table comparing before and after, with time and allocations. Do not include every run.
- Leave out the implementation walkthrough, the list of tests you added, and test pass counts. The diff and CI show those.
- Use the repository's pull request template, and delete template sections that do not apply rather than filling them with "N/A".

## AI Assistance

If an AI tool wrote or substantially shaped the change, say so in the description, in one line. You are still the author. Read and understand every line before submitting, and be ready to explain and revise any of it yourself. Maintainers close pull requests whose submitter cannot.

## During Review

- When you address a comment, reply in a sentence with what you changed, or why you did not. A bare "done" adds nothing.
- Do not resolve a reviewer's thread. The reviewer resolves it once they agree it is fixed.
- Push related fixes together rather than one commit per comment.
- Do not rebase or force-push once review has started unless a maintainer asks.
- When commenting on someone else's pull request, make each comment one specific issue, question, or risk. Leave out praise padding, restated code, and your reasoning steps.
