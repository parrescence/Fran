[← Back to index](index.md)

# FaUiStyleSwitcher

Three buttons — Flow / Terse / Brutal — for the global UI-style axis (see
`_palettes.scss`'s own comment): a fourth, independent axis alongside [palette](palette-switcher.md),
[theme mode](theme-switcher.md), and [input style](input-style-switcher.md), all
stamped as data attributes on `<html>`. Unlike input style, which only retunes the
boxed native-input-like controls, this one retunes the shared shape/type/motion
tokens (radius, font, border-glow shadow, transition speed) that nearly every
component already draws from. No parameters, no Blazor state — picking a button
calls `window.faSetUiStyle(...)` from `theme.js` directly, client-side only, the
same pattern as `FaThemeSwitcher`/`FaPaletteSwitcher`/`FaInputStyleSwitcher`.

## Usage

```razor
<FaUiStyleSwitcher />
```

## The three styles

- **Flow** (default, no attribute needed) — today's look: `'Baloo 2'`-family font,
  8/14/22px radii, a colored "border-glow" shadow on bordered elements, and
  0.15–0.2s transitions.
- **Terse** — a flatter, editorial look: a plain system-font stack, 4/6/10px radii,
  no border-glow shadow (flat borders instead), and no transitions. Status pills
  (badges/chips) keep their pill radius and pick up an uppercase, tracked-out,
  monospace label treatment under Terse.
- **Brutal** — a loud, chunky, neobrutalist look: zero radius everywhere
  (including status pills — the one style where those go square too), a 3px fixed
  dark border on nearly everything, a hard offset "block" shadow (no blur) standing
  in for the glow, and no transitions. Pressing a button/chip shrinks the offset
  instead of just changing color, reading as the element physically pushing down.

Terse and Brutal both ship with system-font fallbacks only — no forced Google
Fonts network request from the library itself. Terse's original reference look
used **Barlow Condensed** (headings), **Libre Franklin** (body), and **IBM Plex
Mono** (labels/numbers); add your own `<link>` for those (same pattern as any other
Google Font) if you want the exact intended look rather than the system-font
fallback.

## Getting the value

Nothing to bind — the current UI style lives in `localStorage` (`fa-ui-style` key)
and the `data-fa-ui-style` attribute on `<html>` (absent for the `flow` default),
both managed by `theme.js`. Read `localStorage.getItem('fa-ui-style')` yourself if
your own code needs it, or call `window.faSetUiStyle('terse')` /
`window.faSetUiStyle('brutal')` / `window.faSetUiStyle(null)` (reset to flow)
directly.

[← Back to index](index.md)
