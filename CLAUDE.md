# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in
this repository.

`FactoryAspects` houses one or more standalone UI **style libraries**, each in its own
top-level folder, each targeting a different framework/architecture. Currently:

- **`Blazor/`** — a Blazor Razor Class Library (C#). See
  [`Blazor/CLAUDE.md`](Blazor/CLAUDE.md) for its component-authoring rules, theming
  system, and package-specific conventions. That file was split out of a private
  monorepo (full history carried over via `git subtree split`).

More libraries for other architectures will land as sibling folders over time (e.g. a
future `React/`). This repo is **public** — don't add anything anywhere in it (this
file, a library's own CLAUDE.md, docs, code comments) that names or describes the
internals of a private app a library was split from, beyond "it was split from one."
The guardrails in each library's own CLAUDE.md apply regardless of which app they
originally came from.

Nothing at the repo root is itself a library — root-level files are repo-wide (this
file, `README.md`, `.github/workflows/`, branch/version policy below). Each library
folder owns everything specific to it: its own `README.md` (packed into that
library's published artifact), `docs/`, `CLAUDE.md`, build/package config, and any
local-only design-reference folders.

## Branching: dev → test → main

Three long-lived branches, one direction of flow, shared across every library in this
repo:

- **`dev`** — the default branch on GitHub, and where day-to-day work happens.
  Feature branches merge here first.
- **`test`** — a gate before `main`. Only reachable via a PR from `dev`.
- **`main`** — what consumers actually pull packages from. Only reachable via a PR
  from `test`. Nothing lands here directly.

`test` and `main` are both branch-protected: no direct pushes, a PR is required, and
the `build-and-pack` CI check (`.github/workflows/ci.yml`) must pass before merging.
Required approving reviews are set to 0 (solo maintainer today) — so a green CI check
is what actually gates the merge, not a second pair of eyes. If collaborators join,
raise `required_approving_review_count` on both branches' protection rules.

## Publishing

`.github/workflows/publish.yml` packs and pushes to GitHub Packages, but **only on
push to `main`**, using the workflow's own `GITHUB_TOKEN` — no manual PAT needed.
`dev`/`test` never publish. `.github/workflows/ci.yml` builds + packs (no publish) on
every push/PR to `dev`/`test`/`main` as a sanity check. After a successful publish,
the same workflow stamps a `vX.Y.Z` git tag on the `main` commit that shipped it
(skipped if it already exists, never re-pointed) — the pinnable target for anything
consuming this repo as source (a git submodule, say) instead of tracking `main`'s
moving tip.

Both workflows currently only build/publish `Blazor/FactoryAspects.csproj` — see that
library's own `CLAUDE.md` for its version-bump policy. Once a second library exists
here, these workflows need to become per-library (independent version/tag, triggered
only by changes under that library's own folder) instead of firing for any push to
`main` regardless of which library changed — don't assume today's single-package
behavior generalizes without updating them first.
