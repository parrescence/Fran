# FinanceApp.UI

A self-contained Blazor Razor Class Library of UI primitives: buttons, cards, alerts,
badges, a modal, a generic N-option toggle/segmented switch, form inputs, page-shell
layouts (header/sidebar/footer), a light/dark/colorblind-safe theme switcher, and a
hand-drawn SVG icon set. No dependency on any other project, database, or web API —
it renders whatever state/callbacks you pass it and nothing else.

Originally built inside the [FinanceApp](https://github.com/bencalvin/FinanceApp)
monorepo (currently the only real consumer, via a project reference), but every
component here is intentionally free of FinanceApp-specific types or assumptions, so
it's usable as-is — either as a NuGet package or by importing the source directly —
in any Blazor WebAssembly or Blazor Server project.

## Install

```
dotnet add package FinanceApp.UI
```

Add the namespaces you want to `_Imports.razor`:

```razor
@using FinanceApp.UI.Components
@using FinanceApp.UI.Icons
@using FinanceApp.UI.Layout
```

### Required manual wiring

Razor Class Libraries ship their static assets under `_content/{PackageId}/...`, but
they are **not** auto-injected into your host page — you must add these tags yourself
(this is standard Blazor RCL behavior, not something specific to this package):

```html
<link rel="stylesheet" href="_content/FinanceApp.UI/css/theme.css" />
...
<script src="_content/FinanceApp.UI/js/theme.js"></script>
<script src="_content/FinanceApp.UI/js/sidebar.js"></script>
```

`theme.js` and `sidebar.js` are plain vanilla-JS IIFEs (no Blazor JS interop, no
external dependencies) that back the theme switcher and the sidebar's collapse toggle;
both persist their state to `localStorage` and stamp classes on `<html>`, so no Blazor
component state needs to stay in sync with them.

`theme.css` pulls one Google Font over `@import` (`Baloo 2`) from
`fonts.googleapis.com`. That's a public CDN URL, not tied to any FinanceApp domain, so
it works from any host — but if your app has a strict Content-Security-Policy or needs
to run fully offline/air-gapped, you'll need to account for or self-host that font.

## What's in here

- **Components**: `FaButton`, `FaCard`, `FaAlert`, `FaBadge`, `FaAvatar`, `FaModal`,
  `FaToggle<TValue>` (pass 2+ `(string Title, TValue Value)` options — see its doc
  comment), `FaInput<TValue>`/`FaSelect<TValue>` (`InputBase<TValue>`-based, for use
  inside an `EditForm`), `ThemeSwitcher`.
- **Layout**: `AppHeader`, `AppFooter`, `AppSidebar`, `SidebarShell` (header + sidebar
  + content + footer), `StandardShell` (header + content + footer, no sidebar).
- **Icons**: `FaIcon` + `FaIconName` — a small hand-drawn SVG set (no icon font/
  external dependency), rendered as inline `<svg>` so `currentColor` picks up
  `FaIconColor`.

## Design notes for contributors

- Nothing in this project may reference `FinanceApp.Shared`, `FinanceApp.Web`, or any
  app-specific type (`UserId`, `AccountId`, `Transaction`, `Category`, ...) — that
  would break its reusability outside this repo. If a component needs data, it takes
  it as a `[Parameter]`.
- No component defaults to a specific brand/app name — `BrandText` on
  `AppHeader`/`AppFooter`/`SidebarShell`/`StandardShell` is `[EditorRequired]` with no
  default value; the consuming app always supplies its own.
- `PackageId` is pinned to `FinanceApp.UI` in the `.csproj` — don't rename it without
  also updating every `_content/FinanceApp.UI/...` reference in every consumer (the
  RCL static-asset path is derived from `PackageId`, not the C# namespace).
