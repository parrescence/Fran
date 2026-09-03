[← Back to index](index.md)

# FaPaletteSwitcher

A single dropdown covering all twenty-eight built-in color palettes, plus any
consumer-supplied ones passed via `CustomPalettes`. Mostly no Blazor state: picking a
built-in option calls `window.faSetPalette(...)` from `theme.js` directly (client-side
only), and every `<FaPaletteSwitcher>` on the page stays in sync with whichever palette
is actually active — including the one restored from `localStorage` on first paint.

This is a separate, independent axis from [FaThemeSwitcher](theme-switcher.md)'s
light/dark/colorblind mode — any palette (built-in or custom) combines with any mode.

**Don't want a picker at all** — just reskinning the app to one fixed brand? Skip this
component entirely and override the `--fa-*` custom properties directly in your own
CSS instead; see [Install & setup, Option D](install.md#5-pick-a-color-palette-optional).
Both approaches read the same tokens, so they combine — a plain-CSS override still
wins over whatever this component last selected, since it loads later in the cascade.

## Usage

```razor
<FaPaletteSwitcher />
```

Typically dropped next to `<FaThemeSwitcher>` in a header.

## Adding your own palettes

The twenty-eight built-ins are precompiled into `fa-styles.css` and selected purely by
name. A palette you define yourself has no compiled CSS to select, so it's expressed
instead as an `FaPalette` object (`FaFa.Components`) and passed via `CustomPalettes`:

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
            // Optional — only the tokens that should actually change in dark mode.
            DarkOverrides: new FaPaletteDarkOverrides(
                Cream: "#14161d", Surface: "#1c1f29", Text: "#eef0f7", TextMuted: "#a3a9ba")),
    ];
}
```

Each `FaPalette` renders as its own `<option value="custom:{Value}">` carrying its
`Colors`/`DarkOverrides` as JSON data attributes. Picking it applies those colors as
inline `--fa-*` custom properties on `<html>` instead of stamping `data-fa-palette` —
same visible effect as a built-in, different mechanism under the hood. `Colors` must
supply every token (the full light-mode set); `DarkOverrides` is optional and sparse —
leave a property `null` to keep that token at its `Colors` value in dark mode too.

`Value` only ever appears prefixed as `"custom:{Value}"`, so it's fine to reuse a
built-in's own slug (e.g. `"southwest-summer"`) for your own palette — they render as
independent options with no collision.

**Caveat:** because a custom palette's colors live in the rendered `<option>`, not in
compiled CSS, the pre-paint "avoid a flash of the wrong palette" snippet
([install.md](install.md#5-pick-a-color-palette-optional)) can't apply one before
Blazor mounts — expect a brief flash of the default (or previously-selected built-in)
palette on reload for a custom selection, resolved once `<FaPaletteSwitcher>` renders
and `theme.js`'s mutation observer re-syncs it.

## Getting the value

Nothing to bind — the current palette lives in `localStorage` (`fa-palette` key, plus
`fa-custom-palette` for a custom selection's colors) and the `data-fa-palette` attribute
on `<html>` (absent for the `northwest-fall` default *and* for any custom palette, which
uses inline `--fa-*` properties instead), both managed by `theme.js`. If your own code
needs to know the current palette, read `localStorage.getItem('fa-palette')` or call
`window.faSetPalette(...)` yourself to change it (built-ins only — a custom palette's
colors have to come from a rendered `<option>`'s data attributes) — see
[Install & setup](install.md#5-pick-a-color-palette-optional) for the full list of
built-in palette names and the build-time alternative.

[← Back to index](index.md)
