---
name: github-actions
description: >-
  Conventions for GitHub Actions workflows in Autofac repositories, including
  the shared reusable CI workflow.
applyTo: "**/.github/workflows/*.yml,**/.github/workflows/*.yaml"
---

# GitHub Actions

Library repositories do not define their own build. Each one has a `.github/workflows/main.yml` that calls the shared `autofac/.github/.github/workflows/ci.yml@main` with `secrets: inherit`, passing only the inputs that differ from the defaults. Fix build behavior in the shared workflow, not by adding steps to one repository.

- A change to the shared workflow runs in every library repository on its next build. Keep inputs backward compatible: add new inputs with defaults, and never rename or remove one that a `main.yml` passes.
- The shared workflow stays self-contained. Inside a reusable workflow, `uses: ./...` resolves against the calling repository, so it cannot call sibling workflows in `autofac/.github`.
- Pass untrusted event data, such as titles, branch names, and bodies, through `env:` rather than interpolating `${{ github.event.* }}` into a `run:` script.
- Give each job the narrowest `permissions:` it needs.
- Comment the reason for anything that is not obvious, such as an event filter, a concurrency group, or a skipped job.
