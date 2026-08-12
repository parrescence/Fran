# Install & setup

Published to **GitHub Packages** (not NuGet.org, for now) under `bencalvin`. Five
steps: add the feed, reference the package, import the namespaces, wire the static
assets into your host page, then optionally pick a color palette.

## 1. Add the GitHub Packages feed as a NuGet source

Pick whichever matches where you're installing from — **local dev** (one-time, per
machine) or **CI** (a GitHub Actions workflow in the same account/org, no PAT needed).

**Local dev:**

```bash
dotnet nuget add source https://nuget.pkg.github.com/bencalvin/index.json \
  --name github-bencalvin \
  --username <your-github-username> \
  --password <a GitHub PAT with read:packages>
```

> **⚠️ Run this yourself in a plain terminal — never through an AI coding assistant's
> terminal/tool-use.** The token passes through that tool's context and, depending on
> the tool, may end up logged, transcripted, or sent to a model provider.

On Windows, leave `--store-password-in-clear-text` off — `dotnet nuget` encrypts the
password at rest by default (Windows DPAPI-backed credential store). Only add that
flag back on Linux/macOS if you don't have a credential provider configured (there's
no OS credential store to fall back on there); if you do, treat `NuGet.Config` as
sensitive — don't commit it, restrict its file permissions.

**CI (same GitHub account/org as this repo):**

```yaml
permissions:
  packages: read
```

```xml
<!-- nuget.config -->
<configuration>
  <packageSources>
    <add key="github-bencalvin" value="https://nuget.pkg.github.com/bencalvin/index.json" />
  </packageSources>
</configuration>
```

The workflow's own `GITHUB_TOKEN` is sufficient — no PAT needed (this repo is private,
but same-account workflows can still read it once granted the permission above).

## 2. Reference the package

```bash
dotnet add package FactoryAspects
```

## 3. Import the namespaces

Add to `_Imports.razor`:

```razor
@using FactoryAspects.Components
@using FactoryAspects.Icons
@using FactoryAspects.Layout
```

## 4. Wire the static assets into your host page

Razor Class Libraries ship their static assets under `_content/{PackageId}/...`, but
they are **not** auto-injected into your host page — this is standard Blazor RCL
behavior, not something specific to this package. Add these tags yourself:

```html
<link rel="stylesheet" href="_content/FactoryAspects/css/theme.css" />
...
<script src="_content/FactoryAspects/js/theme.js"></script>
<script src="_content/FactoryAspects/js/sidebar.js"></script>
```

- `theme.js`/`sidebar.js` are plain vanilla-JS IIFEs (no Blazor JS interop, no external
  dependencies) backing the theme switcher and the sidebar's collapse toggle — both
  persist to `localStorage` and stamp classes on `<html>`, so no Blazor component
  state needs to stay in sync with them.
- `theme.css` pulls one Google Font over `@import` (`Baloo 2`) from
  `fonts.googleapis.com` — a public CDN URL that works from any host, but if your app
  needs a strict CSP or to run fully offline/air-gapped, self-host that font instead.

## 5. Pick a color palette (optional)

Fourteen seasonal/regional/country color palettes ship in the one `theme.css`, picked
via a `data-fa-palette` attribute on `<html>` — a second, independent axis from the
light/dark/colorblind mode (`data-theme`); any palette combines with any mode. Default
is `northwest-fall` (no attribute needed) if you skip this step.

Available names: `northwest-fall`, `southwest-summer`, `northeast-spring`,
`midwest-winter`, `southeast-beach`, `greece-aegean`, `spain-flamenco`,
`ireland-emerald`, `jamaica-blue-mountain`, `japan-indigo`, `korea-celadon`,
`china-cinnabar`, `india-peacock`, `cameroon-rainforest`.

**Option A — pick one at build time**, hardcoded in your host page:

```html
<html lang="en" data-fa-palette="southeast-beach">
```

**Option B — let your app switch palettes at runtime**, by dropping in the bundled
dropdown:

```razor
<PaletteSwitcher />
```

See [PaletteSwitcher](palette-switcher.md) for details. Or call the underlying
function yourself, the same way `<ThemeSwitcher>` calls `window.faSetTheme(...)`:

```js
window.faSetPalette('southeast-beach');
// or window.faSetPalette('northwest-fall') / window.faSetPalette(null) to reset
```

Either way, this persists the choice to `localStorage` under `fa-palette` and stamps
`data-fa-palette` on `<html>`.

**Either way**, add this inline snippet to your host page's `<head>`, **before** the
`theme.css` `<link>` from step 4, so a returning visitor's saved mode/palette applies
before first paint instead of flashing the default and then jumping:

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
