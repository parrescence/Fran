# Install & setup

Published to **GitHub Packages** (not NuGet.org, for now) under `benzaesintese`. Five
steps: add the feed, reference the package, import the namespaces, wire the static
assets into your host page, then optionally pick a color palette.

## 1. Add the GitHub Packages feed as a NuGet source

Pick whichever matches where you're installing from — **local dev** (one-time, per
machine) or **CI** (a GitHub Actions workflow in the same account/org, no PAT needed).

**Local dev:**

```bash
dotnet nuget add source https://nuget.pkg.github.com/benzaesintese/index.json \
  --name github-benzaesintese \
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
    <add key="github-benzaesintese" value="https://nuget.pkg.github.com/benzaesintese/index.json" />
  </packageSources>
</configuration>
```

The workflow's own `GITHUB_TOKEN` is sufficient — no PAT needed. GitHub Packages'
NuGet feeds require authentication to read even for a public repo like this one, but
a same-account workflow's own token satisfies that once granted the permission above.

## 2. Reference the package

```bash
dotnet add package FaFa
```

## 3. Import the namespaces

Add to `_Imports.razor`:

```razor
@using FaFa.Components
@using FaFa.Icons
@using FaFa.Layout
```

## 4. Wire the static assets into your host page

Razor Class Libraries ship their static assets under `_content/{PackageId}/...`, but
they are **not** auto-injected into your host page — this is standard Blazor RCL
behavior, not something specific to this package. Add these tags yourself:

```html
<link rel="stylesheet" href="_content/FaFa/css/fa-styles.css" />
...
<script src="_content/FaFa/js/theme.js"></script>
<script src="_content/FaFa/js/sidebar.js"></script>
```

- `theme.js`/`sidebar.js` are plain vanilla-JS IIFEs (no Blazor JS interop, no external
  dependencies) backing the theme switcher and the sidebar's collapse toggle — both
  persist to `localStorage` and stamp classes on `<html>`, so no Blazor component
  state needs to stay in sync with them.
- `fa-styles.css` pulls one Google Font over `@import` (`Baloo 2`) from
  `fonts.googleapis.com` — a public CDN URL that works from any host, but if your app
  needs a strict CSP or to run fully offline/air-gapped, self-host that font instead.

## 5. Pick a color palette (optional)

Twenty-three seasonal/regional/country color palettes ship in the one `fa-styles.css`,
picked via a `data-fa-palette` attribute on `<html>` — a second, independent axis from
the light/dark/colorblind mode (`data-theme`); any palette combines with any mode.
Default is `northwest-fall` (no attribute needed) if you skip this step.

Available names: `northwest-fall`, `southwest-summer`, `northeast-spring`,
`midwest-winter`, `southeast-beach`, `greece-aegean`, `spain-flamenco`,
`ireland-emerald`, `jamaica-blue-mountain`, `japan-indigo`, `korea-celadon`,
`china-cinnabar`, `india-peacock`, `cameroon-rainforest`, `sahara-desert`,
`brazil-rainforest`, `brazil-favela`, `portugal-tiles`, `spain-bullfighting`,
`mexico-day-of-the-dead`, `london-life`, `new-york-nightlife`, `india-henna`.

**Option A — pick one at build time**, hardcoded in your host page:

```html
<html lang="en" data-fa-palette="southeast-beach">
```

**Option B — let your app switch palettes at runtime**, by dropping in the bundled
dropdown:

```razor
<FaPaletteSwitcher />
```

See [FaPaletteSwitcher](palette-switcher.md) for details. Or call the underlying
function yourself, the same way `<FaThemeSwitcher>` calls `window.faSetTheme(...)`:

```js
window.faSetPalette('southeast-beach');
// or window.faSetPalette('northwest-fall') / window.faSetPalette(null) to reset
```

Either way, this persists the choice to `localStorage` under `fa-palette` and stamps
`data-fa-palette` on `<html>`.

**Either way**, add this inline snippet to your host page's `<head>`, **before** the
`fa-styles.css` `<link>` from step 4, so a returning visitor's saved mode/palette applies
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
