# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in
this repository.

`Fran` houses one or more standalone UI **style libraries**, each in its own
top-level folder, each targeting a different framework/architecture. Currently:

- **`Blazor/`** — a Blazor Razor Class Library (C#). See
  [`Blazor/CLAUDE.md`](Blazor/CLAUDE.md) for its component-authoring rules, theming
  system, and package-specific conventions. That file was split out of a private
  monorepo (full history carried over via `git subtree split`).

More libraries for other architectures will land as sibling folders over time (e.g. a
future `React/`, `Vue/`). This repo is **public** — don't add anything anywhere in it
(this file, a library's own CLAUDE.md, docs, code comments) that names or describes
the internals of a private app a library was split from, beyond "it was split from
one." The guardrails in each library's own CLAUDE.md apply regardless of which app
they originally came from.

Nothing at the repo root is itself a library — root-level files are repo-wide (this
file, `README.md`, `.github/workflows/`, branch/version policy below). Each library
folder owns everything specific to it: its own `README.md` (packed into that
library's published artifact), `docs/`, `CLAUDE.md`, build/package config, and any
local-only design-reference folders.

## Showcase lives in a separate, private repo

The live demo consumer of these libraries — `Showcase.Web.Client` (a standalone
Blazor WebAssembly app) and `Showcase.Web.Api` (an Azure Functions app backing its
"pulled from the database" demos) — is **not in this repo**. It's
[`parrescence/fran-showcase`](https://github.com/parrescence/fran-showcase), a
separate private repo, split out so the demo app can stay non-public while the
libraries here stay open source. It references this repo's `Blazor/Fran.csproj` via
a source-level `<ProjectReference>` rather than the published package (so it always
reflects whatever's on this repo's checked-out branch), which means working on it
locally requires cloning both repos as siblings — see its own README/CLAUDE.md for
details.

**Keep Fran-Showcase in sync with every library change made here.** Any change to a
library that's user-visible — a new component, a renamed component, a new/changed
parameter, a behavior change worth seeing (e.g. a new loading state) — gets a
matching commit to `Fran-Showcase`'s demo page(s) written and pushed to its `dev` in
the same working session, not as a follow-up: since these are two separate repos now
(not one commit spanning both), "in the same change" means push both repos' `dev`
branches together before considering the task done, not just commit locally to one
and move on. It's the live reference for what each library actually does right
now — a change that lands here but not there leaves that reference stale and
defeats the point of having it.

## Branching: dev → test → main

Three long-lived branches, one direction of flow, shared across every library in this
repo:

- **`dev`** — the default branch on GitHub, and where day-to-day work happens.
  Feature branches merge here first.
- **`test`** — a gate before `main`. Only reachable via a PR from `dev`.
- **`main`** — what consumers actually pull packages from. Only reachable via a PR
  from `test`. Nothing lands here directly.

`test` and `main` are both branch-protected: no direct pushes, a PR is required.
Required approving reviews are set to 0 (solo maintainer today) — so green required
CI checks are what actually gate the merge, not a second pair of eyes. If
collaborators join, raise `required_approving_review_count` on both branches'
protection rules.

## CI/publish: one workflow file per library

`.github/workflows/` uses `ci-<library>.yml` / `publish-<library>.yml` naming — one
pair per style library, never a shared workflow that branches internally on which
library changed. `ci-blazor.yml` / `publish-blazor.yml` are the Blazor pair; adding a
new library (say `React/`) means adding `ci-react.yml` / `publish-react.yml` from the
same template, without touching the Blazor files at all.

**CI (`ci-<library>.yml`)**: its job (`build-and-pack-<library>`, e.g.
`build-and-pack-blazor`) is a **required** status check on `test`/`main` branch
protection (one context per library, added there once that library's workflow
exists — check current required contexts with `gh api repos/parrescence/fran/
branches/<branch>/protection/required_status_checks`). Because it's required, the
workflow's **trigger** is deliberately *not* path-filtered to that library's folder —
a PR touching only another library (or root files) would then never fire it, and
GitHub leaves a required-but-never-reported check permanently blocking merge.
Instead the job always runs on every push/PR to `dev`/`test`/`main` (satisfying the
required check for every PR regardless of what it touches), but its actual build/pack
steps are gated behind a `dorny/paths-filter` step scoped to that library's folder
(plus the workflow file itself) and skip — job still reports success — unless
something under that folder actually changed. Copy this pattern exactly for a new
library's `ci-<library>.yml`; don't path-filter the trigger itself.

**Publish (`publish-<library>.yml`)**: packs and pushes to GitHub Packages, but only
on push to `main`, using the workflow's own `GITHUB_TOKEN` — no manual PAT needed.
`dev`/`test` never publish. Its trigger *is* safe to path-filter at the trigger level
(unlike CI) — publish workflows aren't required status checks, so a push to `main`
that doesn't touch that library's folder simply not firing it can't block anything.
After a successful publish, stamps a `vX.Y.Z` git tag on the `main` commit that
shipped it (skipped if it already exists, never re-pointed) — the pinnable target for
anything consuming that library as source instead of tracking `main`'s moving tip.
Each library bumps/publishes its own `<Version>` independently — see that library's
own `CLAUDE.md` for its specific policy (e.g. [`Blazor/CLAUDE.md`](Blazor/CLAUDE.md)).
