---
name: commit
description: >-
  Commit message format for Autofac repositories: one plain-English subject
  that says why, no conventional-commit prefixes, and no AI trailers.
applyTo: "**"
---

# Commit Messages

The subject line says what the change accomplishes or prevents, in plain English. It is what a maintainer sees in `git log` and in release notes years later.

- Use the imperative mood and start with a capital letter: "Prevent", "Allow", "Keep", not "Prevented" or "Fixes".
- Keep it to 72 characters or fewer, with no trailing period.
- Do not use conventional-commit prefixes such as `feat:`, `fix:`, `chore:`, or `refactor(scope):`.
- Do not name files, classes, or methods unless they are the point of the change. Describe the effect.
- An issue number alone is not a subject. Reference the issue in the body.

| Good | Bad |
| --- | --- |
| `Keep Func<T> working on Mono full AOT` | `fix: Func bug` |
| `Do not record a skipped conditional decorator as applied` | `Update DecoratorMiddleware.cs` |
| `Reduce allocations in child scope creation` | `perf(core): optimizations` |

Most commits need no body. Add one, after a blank line, when the reason or a trade-off is not obvious from the subject and the diff. Never retell the diff.

## Rules

- Do not add `Co-authored-by` or similar trailers naming an AI tool or model. The commit belongs to the person submitting it, who must be able to explain every line. Disclose AI assistance in the pull request description instead.
- If the repository has a `.pre-commit-config.yaml`, run `pre-commit run --all-files` and fix what it reports before committing. Do not skip hooks.
- Do not rewrite, amend, or force-push commits someone else may already have pulled unless you are asked to.
