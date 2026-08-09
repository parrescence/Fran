# FactoryAspects

A self-contained Blazor Razor Class Library of UI primitives: buttons, cards, alerts,
badges, a modal, a generic N-option toggle/segmented switch, a full set of form inputs
(text, select, textarea, checkbox, radio group, date picker, file, currency), a table,
page-shell layouts (header/sidebar/footer), a light/dark/colorblind-safe theme switcher,
and a hand-drawn SVG icon set. No dependency on any other project, database, or web API
— it renders whatever state/callbacks you pass it and nothing else.

Every component keeps its original `Fa`-prefixed name (`FaButton`, `FaCard`,
`FaToggle<TValue>`, `FaIcon`, ...) — only the package/namespace/repo identity is
`FactoryAspects`. Every component is a plain C# class (`ComponentBase`/
`InputBase<TValue>` subclass overriding `BuildRenderTree` directly), not `.razor`
markup — see `CLAUDE.md` if you're contributing.

Originally built inside the [FinanceApp](https://github.com/bencalvin/FinanceApp)
monorepo (still an example consumer, now via a package reference instead of an in-repo
project reference) and split out into its own repo/history here. Every component is
intentionally free of FinanceApp-specific types or assumptions, so it's usable as-is —
either as a package or by importing the source directly — in any Blazor WebAssembly or
Blazor Server project.

## Install

Published to **GitHub Packages** (not NuGet.org, for now) under `bencalvin`. Add the
feed as a NuGet source, then reference the package normally:

```
dotnet nuget add source https://nuget.pkg.github.com/bencalvin/index.json \
  --name github-bencalvin \
  --username <your-github-username> \
  --password <a GitHub PAT with read:packages> \
  --store-password-in-clear-text

dotnet add package FactoryAspects
```

(A repo consuming this via CI can skip the manual PAT — see "CI consumers" below.)

Add the namespaces you want to `_Imports.razor`:

```razor
@using FactoryAspects.Components
@using FactoryAspects.Icons
@using FactoryAspects.Layout
```

### Required manual wiring

Razor Class Libraries ship their static assets under `_content/{PackageId}/...`, but
they are **not** auto-injected into your host page — you must add these tags yourself
(this is standard Blazor RCL behavior, not something specific to this package):

```html
<link rel="stylesheet" href="_content/FactoryAspects/css/theme.css" />
...
<script src="_content/FactoryAspects/js/theme.js"></script>
<script src="_content/FactoryAspects/js/sidebar.js"></script>
```

`theme.js` and `sidebar.js` are plain vanilla-JS IIFEs (no Blazor JS interop, no
external dependencies) that back the theme switcher and the sidebar's collapse toggle;
both persist their state to `localStorage` and stamp classes on `<html>`, so no Blazor
component state needs to stay in sync with them.

`theme.css` pulls one Google Font over `@import` (`Baloo 2`) from
`fonts.googleapis.com`. That's a public CDN URL, not tied to any particular domain, so
it works from any host — but if your app has a strict Content-Security-Policy or needs
to run fully offline/air-gapped, you'll need to account for or self-host that font.

### Choosing a theme

One `theme.css`, five seasonal/regional color palettes, picked via a
`data-fa-palette` attribute on `<html>` — same pattern as the existing
light/dark/colorblind mode (`data-theme`), and fully independent of it: any palette
combines with any mode.

- `northwest-fall` — the default. No attribute needed.
- `southwest-summer`
- `northeast-spring`
- `midwest-winter`
- `southeast-beach`

**Pick one at build time** by hardcoding the attribute in your host page:

```html
<html lang="en" data-fa-palette="southeast-beach">
```

**Or let your app switch palettes at runtime**, the same way `<ThemeSwitcher>` calls
`window.faSetTheme(...)`: call `window.faSetPalette('southeast-beach')` (or
`window.faSetPalette('northwest-fall')`/`null` to go back to the default) from a
button's `onclick`. It persists the choice to `localStorage` under `fa-palette` and
stamps `data-fa-palette` on `<html>` — there's no bundled `<PaletteSwitcher>`
component for this yet, so wire your own button(s) up to it for now.

Either way, add this inline snippet to your host page's `<head>`, **before** the
`theme.css` `<link>`, so a returning visitor's saved mode/palette applies before first
paint instead of flashing the default and then jumping:

```html
<script>
  (function () {
    var theme = localStorage.getItem('fa-theme');
    if (theme) document.documentElement.setAttribute('data-theme', theme);
    var palette = localStorage.getItem('fa-palette');
    if (palette) document.documentElement.setAttribute('data-fa-palette', palette);
  })();
</script>
```

### CI consumers

A GitHub Actions workflow in the *same* GitHub account/org as this repo can restore
this package with no manual PAT: declare `permissions: packages: read` in the
workflow, add a `nuget.config` pointing at
`https://nuget.pkg.github.com/bencalvin/index.json`, and the workflow's own
`GITHUB_TOKEN` is sufficient (this repo is private, but same-account workflows can
still read it once granted that permission). See `FinanceApp`'s `ci.yml`/
`deploy-web.yml` for a working example.

## What's in here

- **Components**: `FaButton` (set `Href` to render an anchor styled as a button),
  `FaCard`, `FaAlert`, `FaBadge`, `FaAvatar`, `FaModal`, `FaTable` (typed
  `Columns`/`Rows`, renders a real `<table>`), `FaDataTable<TItem>` (a table that owns
  its own paging/rows-per-page and per-column sorting/filtering — pass `Columns` as
  `FaDataTableColumn<TItem>`, each optionally `Sortable` with a `SortKey`, or given
  `FilterOptions` as `(string Key, string Label)` pairs plus a `FilterPredicate`),
  `FaSearchSelect<TItem>` (type-to-search combobox — debounced `QueryAsync` lookup,
  dropdown of results, no JS interop), `FaDateRange` (linked From/To date fields,
  bound as one `FaDateRangeValue`), `FaToggle<TValue>` (pass 2+
  `(string Title, TValue Value)` options — see its doc comment).
- **Form fields** (all `InputBase<TValue>`-derived, for use inside an `EditForm`):
  `FaInput<TValue>`, `FaSelect<TValue>`, `FaTextarea` (optional maxlength counter /
  read-only display mode), `FaCheckbox`, `FaRadioGroup<TValue>` (same Options-tuple
  shape as `FaToggle<TValue>`), `FaDatePicker` (split day/month/year fields + a
  calendar popup, `Min`/`Max`, optional floating label — no JS interop), `FaFile`
  (optional `AsButton` styled picker), `FaCurrency` (formatted display / raw entry on
  focus, no JS interop).
- **`ThemeSwitcher`**.
- **Layout**: `AppHeader`, `AppFooter`, `AppSidebar`, `SidebarShell` (header + sidebar
  + content + footer), `StandardShell` (header + content + footer, no sidebar).
- **Icons**: `FaIcon` + `FaIconName` — a small hand-drawn SVG set (no icon font/
  external dependency), rendered as inline `<svg>` so `currentColor` picks up
  `FaIconColor`.
