---
paths:
  - "**/*.ps1"
  - "**/*.psm1"
  - "**/*.psd1"
---

# PowerShell

Scripts here run on Windows, macOS, and Linux under PowerShell 7, in CI and on contributor machines, and nobody is around to answer prompts.

## Style

- Name functions and scripts with an approved verb from `Get-Verb` and a singular noun: `Update-AiInstruction`, not `Do-Updates`.
- Use full cmdlet and parameter names. No aliases such as `%`, `?`, `ls`, or `gci`, and no positional parameters except the first argument of a well-known cmdlet.
- Name parameters in `PascalCase` and locals in `camelCase`. Reuse common parameter names such as `Path`, `LiteralPath`, `Force`, and `PassThru` when they mean what the built-in cmdlets mean.
- Build paths with `Join-Path`, never by concatenating strings with `/` or `\`.
- Indent four spaces, with opening braces on the same line.

## Behavior

- Start scripts with `[CmdletBinding()]` and a `param()` block, and use `Set-StrictMode -Version Latest`.
- Take all input through parameters. Never call `Read-Host` or otherwise prompt.
- Add `SupportsShouldProcess` to anything that changes files, repositories, or remote state, and call `$PSCmdlet.ShouldProcess()` right before each change so `-WhatIf` works. Pushing, opening pull requests, and publishing happen only behind an explicit switch.
- Use `-ErrorAction Stop` on calls whose failure must stop the script. Check `$LASTEXITCODE` after every native command, such as `git`, `dotnet`, or `gh`, because PowerShell does not stop on their failure.
- Write data to the output stream as objects. Use `Write-Verbose` for progress, `Write-Warning` for problems, and `Write-Error` or `throw` for failures. Reserve `Write-Host` for interactive summaries.
- Restore any global state you change, such as the current location or an environment variable, in a `finally` block.
- Exit with a nonzero code when anything required fails.

## Help and Checks

- Give every script and exported function comment-based help with `.SYNOPSIS`, `.DESCRIPTION`, every `.PARAMETER`, and at least one `.EXAMPLE`.
- Run PSScriptAnalyzer with the repository's settings file if it has one, and fix what it reports rather than suppressing it.
- Test with Pester 5 where a script has logic worth testing.
