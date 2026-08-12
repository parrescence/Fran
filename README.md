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

## Docs

Per-component usage examples (a minimal `.razor` snippet + how to read the value back
out) live in [`docs/`](docs/index.md) — start at
[`docs/index.md`](docs/index.md) to browse by category, or open
[`docs/site.html`](docs/site.html) directly in a browser for the same content as one
scrollable, searchable-by-scrolling page (no server needed — it's a plain static
file).

## Install

Published to **GitHub Packages** (not NuGet.org, for now) under `bencalvin`. Add the
feed as a NuGet source, then reference the package normally:

```bash
dotnet nuget add source https://nuget.pkg.github.com/bencalvin/index.json \
  --name github-bencalvin \
  --username <your-github-username> \
  --password <a GitHub PAT with read:packages>

dotnet add package FactoryAspects
```

On Windows, leave `--store-password-in-clear-text` off — `dotnet nuget` encrypts the
password at rest (via the Windows DPAPI-backed credential store) by default. On
Linux/macOS there's no equivalent OS credential store, so `dotnet nuget` will refuse
the command without either `--store-password-in-clear-text` (writes the PAT in plain
text into `NuGet.Config`) or a credential provider configured — if you're on one of
those platforms and don't have a credential provider set up, add the flag back in and
treat `NuGet.Config` as sensitive (don't commit it, restrict its file permissions).

> **⚠️ Don't run the `dotnet nuget add source` command above through an AI coding
> assistant's terminal/tool-use.** Running it yourself means the token passes through
> that tool's context — depending on the tool, it may end up logged, transcripted, or
> sent to a model provider. Run it yourself in a plain terminal (your own shell, not
> one an AI agent is driving) — the PAT never needs to touch anything AI-adjacent to
> work.

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

One `theme.css`, fourteen seasonal/regional/country color palettes, picked via a
`data-fa-palette` attribute on `<html>` — same pattern as the existing
light/dark/colorblind mode (`data-theme`), and fully independent of it: any palette
combines with any mode.

- `northwest-fall` — the default. No attribute needed.
- `southwest-summer`
- `northeast-spring`
- `midwest-winter`
- `southeast-beach`
- `greece-aegean`
- `spain-flamenco`
- `ireland-emerald`
- `jamaica-blue-mountain`
- `japan-indigo`
- `korea-celadon`
- `china-cinnabar`
- `india-peacock`
- `cameroon-rainforest`

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
still read it once granted that permission).

## What's in here

- **Components**: `FaButton` (set `Href` to render an anchor styled as a button),
  `FaCard`, `FaAlert`, `FaBadge`, `FaAvatar`, `FaModal`, `FaTable` (typed
  `Columns`/`Rows`, renders a real `<table>`; collapses to a card-per-row layout at
  the same ≤720px breakpoint the sidebar collapses at — see
  [docs/fa-table.md](docs/fa-table.md)), `FaGrid<TItem>` (a table that owns its
  own paging/rows-per-page and per-column sorting/filtering — pass `Columns` as
  `FaGridColumn<TItem>`, each optionally `Sortable` or given `FilterOptions` as
  `(string Key, string Label)` pairs. Two mutually exclusive data modes: `Items`, the
  whole dataset in memory, paged/sorted/filtered client-side; or `ItemsProvider`, a
  `Func<FaGridRequest, Task<FaGridResult<TItem>>>` FaGrid calls on every page/size/
  sort/filter change — it hands back just that page's rows plus a total count, so
  FaGrid never holds more than one page in memory at once, letting a real paged
  query/API back it instead of a fully-loaded list), `FaSearchSelect<TItem>`
  (type-to-search combobox — debounced `QueryAsync` lookup, dropdown of results, no JS
  interop), `FaDateRange` (linked From/To date fields, bound as one
  `FaDateRangeValue`), `FaToggle<TValue>` (pass 2+ `(string Title, TValue Value)`
  options — see its doc comment).
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
