[← Back to index](index.md)

# ThemeSwitcher

Three explicit theme buttons — Light / Dark / Colorblind-safe. No parameters, no
Blazor state: clicking calls `window.faSetTheme(...)` from `theme.js` directly
(client-side only), and which button looks "active" is tracked by that same script.

## Usage

```razor
<ThemeSwitcher />
```

Typically dropped into a header, same as `AppHeader` does internally — see
[Layout shells](layout-shells.md).

## Getting the value

Nothing to bind — the current theme lives in `localStorage` (`fa-theme` key) and the
`data-theme` attribute on `<html>`, both managed by `theme.js`. If your own code needs
to know the current theme, read `localStorage.getItem('fa-theme')` or call
`window.faSetTheme(...)` yourself to change it. For picking a color palette (a
separate, independent axis from theme mode), see [PaletteSwitcher](palette-switcher.md)
or [Install & setup](install.md#5-pick-a-color-palette-optional).

[← Back to index](index.md)
