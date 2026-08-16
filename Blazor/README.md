# FaFa

A self-contained Blazor Razor Class Library of UI primitives — components, form
inputs, layout shells, a theme switcher, and an icon set. No dependency on any other
project, database, or web API — it renders whatever state/callbacks you pass it and
nothing else.

Blazor is a C#/.NET UI framework — this library only works in a .NET project (Blazor
Server or WebAssembly). It is not usable from JavaScript/TypeScript, React, Vue, or
any non-.NET frontend.

Every component keeps its original `Fa`-prefixed name (`FaButton`, `FaCard`,
`FaToggle<TValue>`, `FaIcon`, ...) — only the package/namespace/repo identity is
`FaFa`. Every component is a plain C# class (`ComponentBase`/
`InputBase<TValue>` subclass overriding `BuildRenderTree` directly), not `.razor`
markup — see `CLAUDE.md` if you're contributing.

## Install

Published to **GitHub Packages** (not NuGet.org, for now) under `benzaesintese`:

```bash
dotnet nuget add source https://nuget.pkg.github.com/benzaesintese/index.json \
  --name github-benzaesintese --username <your-github-username> \
  --password <a GitHub PAT with read:packages>

dotnet add package FaFa
```

That's the package reference. The library also ships CSS/JS static assets that Blazor
doesn't auto-wire into your host page, plus 23 optional color palettes — full
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
