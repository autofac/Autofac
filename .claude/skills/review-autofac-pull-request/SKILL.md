---
name: review-autofac-pull-request
description: >-
  Deep review of one change to an Autofac repository. Runs independent
  reviewers over the diff, one per concern (compatibility, platform reach,
  concurrency, performance against maintenance cost, tests, and more), checks
  every candidate finding with a separate verifier, and reports only the ones
  that hold up. Use when reviewing an Autofac pull request, a branch, or
  uncommitted changes, including a re-review after new commits.
argument-hint: "[low|medium|high|max] [<pr number or URL>|<branch>|<path>] [--post]"
---

# Review Autofac Pull Request

## Intent

A review a maintainer can trust enough to act on. Several reviewers read the same change, each looking for one kind of problem, and each candidate finding is then checked by a verifier that sees only that finding. The fan-out gives depth, and the verification keeps false positives out. A short list of real problems is worth more than a long list nobody believes.

Use it to review a pull request to any Autofac repository, your own branch before opening one, or uncommitted changes. It reviews whether a change is correct. It does not audit a repository's structure.

## Workflow

Run every phase in one continuous pass. The review is not done until the report in Phase 5 exists, so do not stop to post progress while reviewers or verifiers are still running.

### Phase 1: Decide Whether to Review

When the invoker named a target, review it, whatever its state. Note in one line if it is merged, closed, or a draft.

When you picked the target yourself, skip it and say why if every commit is from a bot such as Dependabot or pre-commit, if your review is already on the current head SHA, or if the diff is only noise the reviewer agent is told to ignore.

### Phase 2: Gather the Change

Collect these once and write them to the scratch directory, so every reviewer reads the same material:

1. The full diff and the head SHA. For a pull request: `gh pr diff <n> -R autofac/<repo>` and `gh pr view <n> -R autofac/<repo> --json headRefOid,title,body,baseRefName,files`.
2. The paths of the repository's rule files: `AGENTS.md`, `.claude/rules/`, `.github/instructions/`, any `CONTRIBUTING.md`, `.editorconfig`, and the analyzer configuration under `build/`. The shared contributor guide is in the `autofac/.github` repository.
3. The title and description, verbatim.
4. Each linked issue, from `Fixes #n` or `Refs #n` or the pull request's linked issues, with `gh issue view <n> -R autofac/<repo> --comments`. The issue discussion often records a maintainer's design decision, which the change must follow.
5. The reviewable file list, with noise left out.
6. Anything the invoker told you about intent, deliberate choices, or follow-up work. Reviewers cannot see this conversation.
7. Your own unresolved review threads on the pull request, with their replies.

### Phase 3: Fan Out

Spawn the `autofac-pull-request-reviewer` agent once per lane in the panel, all in a single message, each with its focus directive and the Phase 2 paths. Name the lane in the directive exactly as the agent's Lanes section does.

| Lane | Run when |
| --- | --- |
| Correctness | Always |
| Compatibility | Always |
| Tests | Always |
| Disclosure | Always |
| Concurrency | The change touches shared state, caching, locking, lifetime scopes, or disposal |
| Platform | The change touches code compiled for more than one target framework, or reflection and code generation |
| Performance | The change touches the resolve pipeline, scope creation, container build, or claims a performance gain |
| Design and Maintenance | The change adds public API, an option, a new type, or more than about fifty lines of logic |
| Error Handling | The change adds or changes exceptions, `catch` blocks, or argument validation |
| Security | The change loads types or assemblies, reads configuration, adds a cache, or edits a workflow |
| Dependencies and Downstream | The change edits a project file or a public API that other Autofac repositories use |
| Documentation | The change adds or rewrites comments, XML documentation, or prose |
| Earlier threads | You have unresolved threads on the pull request. Directive: judge only whether the change at head closes each thread, and quote the line that does. |

Size the panel by effort:

| Effort | Panel |
| --- | --- |
| `low` | Correctness, Compatibility, and Disclosure. |
| `medium` | `low` plus Tests and every conditional lane whose condition holds. |
| `high` (default) | `medium`. |
| `max` | Every lane, regardless of condition. Then, after Phase 4, one more reviewer with the surviving findings and the directive "Reread the diff. Report only defects not already listed." Verify its findings the same way. |

Record every lane you skipped and why, for the report.

If your environment cannot spawn subagents, work through the same lanes yourself one at a time, writing each lane's findings down before starting the next.

#### Scratch Directory and Collection

Use `${TMPDIR:-/tmp}/autofac-review/<repo>-<short-sha>`, so an interrupted run can find what earlier agents wrote. Give each agent a report path inside it and tell it to write its report there as its last action. Collect by reading the files, not by waiting on replies, because a completion message can be lost while you are busy. If a file is missing, wait and check again. Respawn an agent only after it has stopped without writing its file, and only once.

### Phase 4: Verify

Send each candidate finding to its own verifier with the diff and the rule file paths, in parallel, collected the same way. Give each verifier this rubric:

> Score from 0 to 100 how confident you are that this finding is real. Do not consider how important it is.
>
> - 0: false, or about code the change did not touch, or something the reviewer was told never to report.
> - 25: plausible but unverified. For a convention finding, the quoted rule does not actually say this.
> - 50: partly verified, with one link in the argument still assumed.
> - 85: verified against the diff, the repository, or its history.
> - 100: verified, and the evidence confirms every part.
>
> A claim that something is missing, such as a test or a check, scores 25 or less unless the reviewer says where they looked. A compatibility finding about a public member stands even if no caller is visible locally, because Autofac's consumers are not in this repository. A finding that only says the description is short scores 0.

Keep findings scoring 80 or more. Drop anything a maintainer already settled in an earlier review unless the code has changed since. Merge duplicates, keeping the most specific version. Verify each earlier thread the reviewer judged closed the same way, and treat it as still open below 80.

Do not mention scores, verifiers, or dropped findings in the report.

### Phase 5: Report

If a reviewer refuted a concern the change invites, and a verifier confirmed the refutation, say so first. For example: the odd ordering matches a documented contract, or the missing check exists one layer up.

Then list the findings, numbered, most severe first:

```markdown
### 1. Scope disposal races with a concurrent resolve - High, concurrency

`src/Autofac/Core/Lifetime/LifetimeScope.cs:212` · [permalink](https://github.com/autofac/Autofac/blob/<full-sha>/src/Autofac/Core/Lifetime/LifetimeScope.cs#L211-L213)

Evidence: the input, state, or interleaving and what goes wrong.

Fix: what to change, starting with a verb.
```

Follow with any cross-cutting findings, then earlier threads closed and still open. Then one line on which lanes you skipped and why.

End with one recommendation, consistent with the severities above it:

- **Request changes** when any High or Medium finding remains. Name them.
- **Approve** when only Low findings or none remain, and say the Lows are optional.
- **Comment** only when another reviewer's open request for changes already covers every High and Medium.

Nothing else: no summary of the change, no list of what was fine.

## Inputs and Outputs

Inputs come from the invoking text:

- **Effort**: `low`, `medium`, `high`, or `max`. The default is `high`.
- **Target**: a pull request number or URL; a path, which limits the review to those files; a branch, compared with `git diff <default>...<branch>`; or nothing, which reviews the current branch's open pull request, or its diff against the default branch plus uncommitted changes. The default branch is `develop` for libraries and `main` for Documentation, Examples, and `.github`. Ask only if none of these gives a diff.
- **`--post`**: after the report, post it as one pull request review whose verdict is the recommendation, with each anchored finding as an inline comment. GitHub refuses a verdict on your own pull request, so post it as a comment there and say so.
- **Context** the reviewers should know, such as deliberate choices or planned follow-up work.

Output is the report, in the conversation. Agent reports go to the scratch directory, outside every working tree.

## Safety

- Read only unless `--post` is given or the invoker asks to post. The one exception is resolving your own earlier threads that a verifier confirmed are fixed.
- Never edit, commit, or push anything in the repository under review.
- Do not run builds, tests, or linters. CI owns that signal, and a local environment problem is not a finding about someone's code.
- Do not pad the report. An empty review is a valid result.
- Never print a credential. Cite where it is.
- The recommendation is advice. A maintainer decides what merges.
