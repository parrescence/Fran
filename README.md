# FactoryAspects

A self-contained Blazor Razor Class Library of UI primitives — components, form
inputs, layout shells, a theme switcher, and an icon set. No dependency on any other
project, database, or web API — it renders whatever state/callbacks you pass it and
nothing else.

Every component keeps its original `Fa`-prefixed name (`FaButton`, `FaCard`,
`FaToggle<TValue>`, `FaIcon`, ...) — only the package/namespace/repo identity is
`FactoryAspects`. Every component is a plain C# class (`ComponentBase`/
`InputBase<TValue>` subclass overriding `BuildRenderTree` directly), not `.razor`
markup — see `CLAUDE.md` if you're contributing.

## Install

Published to **GitHub Packages** (not NuGet.org, for now) under `bencalvin`:

```bash
dotnet nuget add source https://nuget.pkg.github.com/bencalvin/index.json \
  --name github-bencalvin --username <your-github-username> \
  --password <a GitHub PAT with read:packages>

dotnet add package FactoryAspects
```

That's the package reference. The library also ships CSS/JS static assets that Blazor
doesn't auto-wire into your host page, plus 14 optional color palettes — full
step-by-step (including the CI-friendly no-PAT path, and the exact host-page tags to
add) is in **[`docs/install.md`](docs/install.md)**.

## What's available

Buttons, cards, alerts, badges, an avatar, a modal, a generic N-option toggle, a full
set of form inputs (text, select, search-select, textarea, checkbox, radio group, date
picker, date range, file, currency), a plain table and a paged/sortable/filterable
grid, page-shell layouts (header/sidebar/footer), a light/dark/colorblind-safe theme
switcher, and a hand-drawn SVG icon set — see **[`docs/index.md`](docs/index.md)** for
the full inventory with usage examples for each, or open
[`docs/site.html`](docs/site.html) in a browser for the same content as one
scrollable page (no server needed).

> `docs/` links above are relative, so they resolve correctly no matter which branch
> you're viewing on GitHub (`dev`/`test`/`main`) — but they won't resolve from the
> README as rendered on the GitHub Packages package page, since only `README.md`
> itself is packed into the `.nupkg`, not `docs/`. Browse the repo on GitHub for the
> full docs.
