# Install & setup

Published to **GitHub Packages** (not NuGet.org, for now) under `parrescence`. Five
steps: add the feed, reference the package, import the namespaces, wire the static
assets into your host page, then optionally pick a color palette — plus one more,
optional, if you're using `FaToastHost`.

## 1. Add the GitHub Packages feed as a NuGet source

Pick whichever matches where you're installing from — **local dev** (one-time, per
machine) or **CI** (a GitHub Actions workflow in the same account/org, no PAT needed).

**Local dev:**

```bash
dotnet nuget add source https://nuget.pkg.github.com/parrescence/index.json \
  --name github-parrescence \
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
    <add key="github-parrescence" value="https://nuget.pkg.github.com/parrescence/index.json" />
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
<script src="_content/FaFa/js/codeblock.js"></script>
<script src="_content/FaFa/js/fa-date-wheel.js"></script>
```

- `theme.js`/`sidebar.js`/`codeblock.js` are plain vanilla-JS IIFEs (no Blazor JS
  interop, no external dependencies) backing the theme switcher, the sidebar's
  collapse toggle, and `<FaCodeBlock>`'s copy-to-clipboard button — all persist to
  `localStorage` and/or touch the DOM directly, so no Blazor component state needs
  to stay in sync with them. Skip the `codeblock.js` tag if you never use
  `<FaCodeBlock Copyable="true">` (the default) — without it, the copy button
  renders but clicking it does nothing.
- `fa-date-wheel.js` is the same kind of plain vanilla-JS IIFE, backing `<FaDate>`'s
  compact wheel picker (see `fa-date.md`) — watches each wheel and, once scrolling
  settles, clicks whichever value ended up centered, running that click through the
  button's own Blazor `onclick` exactly as a manual click would (no JS-to-Blazor
  interop call in the file at all). Skip this tag if you never use `<FaDate>`'s
  compact picker — without it, the wheels still scroll (plain CSS), the number
  inputs above them still work, and tapping a value directly still works; only
  scroll-to-select stops updating the value on its own.
- `fa-styles.css` pulls one Google Font over `@import` (`Baloo 2`) from
  `fonts.googleapis.com` — a public CDN URL that works from any host, but if your app
  needs a strict CSP or to run fully offline/air-gapped, self-host that font instead.

## 5. Pick a color palette (optional)

Twenty-eight seasonal/regional/country color palettes ship in the one `fa-styles.css`,
picked via a `data-fa-palette` attribute on `<html>` — a second, independent axis from
the light/dark/colorblind mode (`data-theme`); any palette combines with any mode.
Default is `northwest-fall` (no attribute needed) if you skip this step.

Available names: `northwest-fall`, `southwest-summer`, `northeast-spring`,
`midwest-winter`, `southeast-beach`, `greece-aegean`, `spain-flamenco`,
`ireland-emerald`, `jamaica-blue-mountain`, `japan-indigo`, `korea-celadon`,
`china-cinnabar`, `india-peacock`, `cameroon-rainforest`, `sahara-desert`,
`brazil-rainforest`, `brazil-favela`, `portugal-tiles`, `spain-bullfighting`,
`mexico-day-of-the-dead`, `london-life`, `new-york-nightlife`, `india-henna`, `ruckus`,
`dinner`, `hang-in`, `floating`, `fit`.

**Option A — pick one at build time**, hardcoded in your host page:

```html
<html lang="en" data-fa-palette="southeast-beach">
```

**Option B — let your app switch palettes at runtime**, by dropping in the bundled
dropdown:

```razor
<FaPaletteSwitcher />
```

**Option C — bring your own palette at runtime**, in addition to the twenty-eight
built-ins, by passing `FaPalette` objects to `FaPaletteSwitcher`'s `CustomPalettes`
parameter:

```razor
<FaPaletteSwitcher CustomPalettes="_myPalettes" />

@code {
    private readonly FaPalette[] _myPalettes =
    [
        new FaPalette(
            Value: "acme-brand",
            Label: "Acme Brand",
            Colors: new FaPaletteColors(
                Primary: "#2454ff", PrimaryDark: "#1638b0", PrimaryLight: "#7a9bff",
                Footer: "#eef1fb", Glow: "#3f6bff", Gold: "#d9a441", Accent: "#00a389",
                AccentDark: "#00786a", Ember: "#c0392b", Wine: "#6b1f2a", Fir: "#20242b",
                Cream: "#f7f8fc", Surface: "#ffffff", Text: "#1b1f2a", TextMuted: "#5c6270",
                TextOnPrimary: "#ffffff", Border: "#c9d2ec", BorderFocus: "#1638b0",
                AlertDangerBg: "#fbeaea", AlertSuccessBg: "#e3f5f1", AlertInfoBg: "#eaf0fc"),
            DarkOverrides: new FaPaletteDarkOverrides(
                Cream: "#14161d", Surface: "#1c1f29", Text: "#eef0f7", TextMuted: "#a3a9ba")),
    ];
}
```

A custom palette has no compiled CSS of its own — unlike a built-in, its colors travel
as JSON on the `<option>` itself and get applied as inline `--fa-*` custom properties on
`<html>` at selection time (see [FaPaletteSwitcher](palette-switcher.md)). Only
`Colors` is required; `DarkOverrides` is optional and only needs the tokens that should
actually change in dark mode.

See [FaPaletteSwitcher](palette-switcher.md) for details on Options B/C. Or call the
underlying function yourself, the same way `<FaThemeSwitcher>` calls
`window.faSetTheme(...)` — built-ins only, since a custom palette's colors have to come
from a rendered `<option>`'s data attributes:

```js
window.faSetPalette('southeast-beach');
// or window.faSetPalette('northwest-fall') / window.faSetPalette(null) to reset
```

Either way, this persists the choice to `localStorage` under `fa-palette` (a custom
palette's colors also go into `fa-custom-palette`) and stamps `data-fa-palette` on
`<html>` (skipped for a custom palette — see below).

**Option D — override colors directly with your own CSS**, no picker at all. Every
component reads color exclusively through the `--fa-*` custom properties `:root`
defines (see `Blazor/CLAUDE.md`'s "Themes" section) — so redeclaring any of them in
your own stylesheet, loaded **after** `fa-styles.css`, silently wins over whichever
built-in palette is active, no `FaPalette`/C# involved:

```css
/* your-app.css, linked after fa-styles.css */
:root {
  --fa-primary: #2454ff;
  --fa-primary-dark: #1638b0;
  --fa-accent: #00a389;
  /* ...override only the tokens you actually want to change; every token this
     omits keeps coming from whichever built-in palette is active. */
}
```

Options A–C are all for a palette a user can *pick* — one fixed choice hardcoded at
build time (A), one chosen at runtime from the bundled dropdown, built-in (B) or
your own addition to it (C). Option D is for reskinning the app to one brand with no
picker UI at all, and needs nothing from this library beyond the `--fa-*` names
themselves — see `_palettes.scss` in this repo, or any rendered palette's swatch page
under `Showcase`'s `/palette` gallery, for the full token list and what each one
controls. It combines fine with A/B/C too: your override CSS simply wins over
whichever built-in/custom palette happens to be active for any token it redeclares,
since it loads later in the cascade — handy for "start from a built-in palette but
tweak two or three tokens" without hand-copying the other eighteen.

**Whichever of A–C you use**, add this inline snippet to your host page's `<head>`, **before** the
`fa-styles.css` `<link>` from step 4, so a returning visitor's saved mode/palette applies
before first paint instead of flashing the default and then jumping:

```html
<script>
  (function () {
    var theme = localStorage.getItem('fa-theme');
    if (theme) document.documentElement.setAttribute('data-theme', theme);
    var palette = localStorage.getItem('fa-palette');
    // A "custom:..." palette has no compiled CSS to select via the attribute — its
    // colors get applied as inline custom properties by theme.js once it loads
    // (Blazor mounts <FaPaletteSwitcher> after this snippet runs), so there's a
    // brief flash of the default palette for that case only; skip the attribute here
    // rather than stamping a value nothing selects on.
    if (palette && palette.indexOf('custom:') !== 0) {
      document.documentElement.setAttribute('data-fa-palette', palette);
    }
  })();
</script>
```

## 6. Customize typography & shape (optional)

`--fa-font` is a single global custom property (same architecture as the color
tokens above) — every component reads its font through it, so swapping it in your
own stylesheet, loaded **after** `fa-styles.css`, reskins the whole library's
typography with no FaFa code involved:

```css
/* your-app.css, linked after fa-styles.css */
:root {
  --fa-font: 'Inter', system-ui, sans-serif;
}
```

A handful of other palette-agnostic "standard" tokens work the same way — defined
once in `:root`, reused everywhere, and safe to override the same way:

| Token | Default | Controls |
| --- | --- | --- |
| `--fa-radius-sm` / `-md` / `-lg` / `-pill` | `8px` / `14px` / `22px` / `999px` | Corner rounding — small controls, cards/panels, large surfaces, pill shapes |
| `--fa-border-width` | `2px` | Standard border weight everywhere a component draws one |
| `--fa-transition-fast` / `-medium` | `0.15s ease` / `0.2s ease` | Hover/focus color & shadow changes vs. size/layout changes (sidebar collapse, toggle width) |

**Not** overridable this way: per-component padding/margin/gap and most font-size/
font-weight values. Those are deliberately hand-tuned per component rather than
drawn from a shared scale (see `Blazor/CLAUDE.md`'s "Themes" section) — two
components using different spacing isn't drift to fix, it's the design. To change
one of those, target that component's own class (`.fa-btn`, `.fa-card`, ...) in your
own CSS instead of looking for a token.

## 7. Set a favicon / app icon (optional)

This library ships no favicon of its own — like `BrandText`, it's your app's own
branding, not something a style library should hardcode (see `Blazor/CLAUDE.md`'s
"no hardcoded brand/app defaults" rule). Add the usual `<link>` tags to your host
page's `<head>` yourself, the same as any Blazor app:

```html
<link rel="icon" type="image/png" href="favicon.png" />
<link rel="apple-touch-icon" href="apple-touch-icon.png" />
```

If you want a logo next to your app's name in the header/sidebar chrome itself
(distinct from the browser-tab favicon above), that's `BrandIconUrl` on
`<FaHeader>`/the page shells/templates — see
[Layout shells](layout-shells.md#parameters-shared-by-both-shells) and
[Page templates](page-templates.md).

## 8. Using FaToastHost (optional)

Every other component in this library needs zero C#-side setup — import the
namespace and use the tag. `FaToastHost`/`FaToastService` is the one exception:
register the service once in `Program.cs` before mounting `<FaToastHost />`.

```csharp
builder.Services.AddScoped<FaToastService>();
```

See [FaToastHost](fa-toast.md) for why it's `Scoped` and how to fire a toast from
anywhere in your app.
