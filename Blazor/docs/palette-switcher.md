[← Back to index](index.md)

# FaPaletteSwitcher

A single dropdown covering all twenty-eight color palettes. No parameters, no Blazor
state: picking an option calls `window.faSetPalette(...)` from `theme.js` directly
(client-side only), and every `<FaPaletteSwitcher>` on the page stays in sync with
whichever palette is actually active — including the one restored from
`localStorage` on first paint.

This is a separate, independent axis from [FaThemeSwitcher](theme-switcher.md)'s
light/dark/colorblind mode — any palette combines with any mode.

## Usage

```razor
<FaPaletteSwitcher />
```

Typically dropped next to `<FaThemeSwitcher>` in a header.

## Getting the value

Nothing to bind — the current palette lives in `localStorage` (`fa-palette` key) and
the `data-fa-palette` attribute on `<html>` (absent for the `northwest-fall` default),
both managed by `theme.js`. If your own code needs to know the current palette, read
`localStorage.getItem('fa-palette')` or call `window.faSetPalette(...)` yourself to
change it — see [Install & setup](install.md#5-pick-a-color-palette-optional) for the
full list of palette names and the build-time alternative.

[← Back to index](index.md)
