# Fran

A self-contained Blazor Razor Class Library of UI primitives — components, form
inputs, layout shells, a theme switcher, and an icon set. No dependency on any other
project, database, or web API — it renders whatever state/callbacks you pass it and
nothing else.

Blazor is a C#/.NET UI framework — this library only works in a .NET project (Blazor
Server or WebAssembly). It is not usable from JavaScript/TypeScript, React, Vue, or
any non-.NET frontend.

Every component keeps its original `Fa`-prefixed name (`FaButton`, `FaCard`,
`FaToggle<TValue>`, `FaIcon`, ...) — only the package/namespace/repo identity is
`Fran`. Every component is a plain C# class (`ComponentBase`/
`InputBase<TValue>` subclass overriding `BuildRenderTree` directly), not `.razor`
markup — see `CLAUDE.md` if you're contributing.

## Install

Published to **GitHub Packages** (not NuGet.org, for now) under `parrescence`:

```bash
dotnet nuget add source https://nuget.pkg.github.com/parrescence/index.json \
  --name github-parrescence --username <your-github-username> \
  --password <a GitHub PAT with read:packages>

dotnet add package Fran
```

That's the package reference. The library also ships CSS/JS static assets that Blazor
doesn't auto-wire into your host page, plus 28 optional color palettes — pick one,
switch between them at runtime, add your own alongside them, or override the color
tokens directly with your own CSS — full step-by-step (including the CI-friendly
no-PAT path, and the exact host-page tags to add) is in
**[`docs/install.md`](docs/install.md)**.

## What's available

Buttons, cards, alerts, badges, an avatar, a modal, a generic N-option toggle, a full
set of form inputs (text, select, search-select, textarea, checkbox, radio group, date
[+ calendar/wheel picker], date range, file, currency), a three-tier validation system
(root/DTO, form, and per-element, with inline error messages on every field), a plain
table and a paged/sortable/filterable grid, a carousel, code blocks, tabs, an
accordion, breadcrumbs, pagination, a divider, chips, an empty-state placeholder,
tooltips, a popover, toast notifications, a skeleton loader, page-shell layouts
(header/sidebar/footer), four full-page templates built on those shells (dashboard,
form, home, auth), 28 color palettes, a light/dark/colorblind-safe theme switcher, and
a hand-drawn SVG icon set — see **[`docs/index.md`](docs/index.md)** for the full
inventory with usage examples for each, or open [`docs/site.html`](docs/site.html) in
a browser for the same content as one scrollable page (no server needed).
