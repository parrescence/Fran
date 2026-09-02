[← Back to index](index.md)

# FaInputStyleSwitcher

Three buttons — Standard / Minimal / Maximal — for the global input-style axis (see
[Sizing & responsive](sizing.md#input-style-standard--minimal--maximal)): a
third, independent axis alongside [palette](palette-switcher.md) and
[theme mode](theme-switcher.md), all stamped as data attributes on `<html>`. Unlike
`FaInput`/etc.'s own per-instance `Size`, this retunes every boxed native-input-like
control (`FaInput`, `FaSelect`, `FaTextarea`, `FaCurrency`) app-wide, with nothing to
set per component instance. No parameters, no Blazor state — picking a button calls
`window.faSetInputStyle(...)` from `theme.js` directly, client-side only, the same
pattern as `FaThemeSwitcher`/`FaPaletteSwitcher`.

## Usage

```razor
<FaInputStyleSwitcher />
```

## The three styles

- **Standard** (default, no attribute needed) — today's look: `var(--fa-border-width)`
  border, `var(--fa-radius-md)` corners, the usual soft glow shadow.
- **Minimal** — 1px border, square corners (no radius), no shadow.
- **Maximal** — 3px border, `var(--fa-radius-lg)` corners.

Only `FaInput`/`FaSelect`/`FaTextarea`/`FaCurrency` participate — the flattened
controls (`FaCheckbox`, `FaToggle`, `FaDate`, `FaSearchSelect`, `FaDropdown`, ...)
were never boxed the same way to begin with, so this axis doesn't touch them.

## Getting the value

Nothing to bind — the current input style lives in `localStorage` (`fa-input-style`
key) and the `data-fa-input-style` attribute on `<html>` (absent for the `standard`
default), both managed by `theme.js`. Read
`localStorage.getItem('fa-input-style')` yourself if your own code needs it, or call
`window.faSetInputStyle('minimal')` / `window.faSetInputStyle('maximal')` /
`window.faSetInputStyle(null)` (reset to standard) directly.

[← Back to index](index.md)
