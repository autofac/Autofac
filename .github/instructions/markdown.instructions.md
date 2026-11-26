---
name: markdown
description: >-
  Markdown style for Autofac READMEs, guides, and GitHub templates, beyond
  what markdownlint enforces.
applyTo: "**/*.md"
---

# Markdown

Markdown here must pass markdownlint with the repository's `.markdownlint.json`. pre-commit runs it. These rules cover what the linter cannot check.

## Structure

- Use one `#` heading as the document title, and nest headings in order without skipping a level.
- Write headings in Title Case. Do not use bold text in place of a heading.
- Do not create a sub-heading with no siblings, or a list with only one item. Fold it into the text above.
- Put introductory text directly under the title rather than in an "Overview" section.

## Formatting

- Never hard-wrap prose. Each paragraph and each list item is one line, however long.
- Use `-` for bullets and `1.` for numbered lists, and indent nested lists to the parent item's text.
- Give every fenced code block a language, such as `csharp`, `xml`, `powershell`, or `text`.
- Put code, type names, file paths, and commands in backticks.
- Avoid raw HTML. GitHub renders Markdown, and HTML makes the source harder to read.

## Content

- Link text must make sense out of context. Link the words "contributor guide", never the word "here". Do not leave bare URLs in prose.
- Give every image alt text that describes what it shows.
- Write short sentences in the active voice, with common words.
- Start each list item with a capital letter.
- Use emoji rarely, never as list markers, and never as the only way something is said.
- Write "ID", not "Id" or "id", in prose, and write "and" rather than "&".
