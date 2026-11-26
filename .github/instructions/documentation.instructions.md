---
name: documentation
description: >-
  Keeps comments, documentation, commit messages, issues, and pull request
  descriptions short enough that maintainers read them.
applyTo: "**"
---

# Writing for People

Maintainers read every comment, issue, and pull request themselves, in their spare time. Writing that is twice as long as it needs to be gets read half as carefully. These rules apply to code comments, XML documentation, Markdown and reStructuredText, commit messages, issues, and pull request descriptions.

- Lead with why. The code shows what changed and how it works.
- Comment only where the reason for the code is not evident from the code: a constraint, a trade-off, an issue number for a regression test, or something a future reader might "fix" and break.
- Say each thing once. No summary that repeats the bullet list, and no closing paragraph that repeats the summary.
- Prefer a sentence to a paragraph, and a paragraph to a new section. A heading over two lines of text is not needed.
- Do not copy the code into prose. No lists of the files you changed, the methods you added, or the values you set. They go stale, and the diff already shows them.
- Point to things instead of repeating them: "the defaults are in `ContainerBuilderOptions`" rather than a table of every default.
- No preamble, no restating the request, and no wrap-up.
- Use US English spelling: behavior, color, canceled, initialize.

Brevity is not emptiness. XML documentation for public API still has to tell the reader something the member name does not: a parameter documented as "The index." helps nobody. Write the documentation you would want to read the first time you used the feature.

Length should follow how hard the change is. A subtle concurrency fix earns a longer explanation than a typo fix. Repetition never earns length.

Before presenting any of this writing, reread the draft against these rules and cut what they say to cut. A first draft almost always has something to remove.
