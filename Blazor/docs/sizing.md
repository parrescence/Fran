[← Back to index](index.md)

# Sizing & responsive

Two cross-cutting concepts, not single components — like [Validation](validation.md),
this page documents a pattern shared across several components rather than one tag.

## `Size` (the `FaSize` scale)

A shared five-step enum — `XSmall` \| `Small` \| `Medium` (default) \| `Large` \|
`XLarge` — available as a `Size` parameter on:

`FaButton`, `FaInput<TValue>`, `FaSelect<TValue>`, `FaTextarea`, `FaCheckbox`,
`FaCurrency`, `FaCard`, `FaAlert`, `FaBadge`, `FaChip`.

```razor
<FaButton Variant="FaButtonVariant.Primary" Size="FaSize.Small">Small</FaButton>
<FaInput @bind-Value="_search" Label="Search" Size="FaSize.Large" />
```

`Medium` renders with none of the size classes below — it's the exact look every one
of these components already had before `Size` existed, so adding `Size` to a
component is never a visual change for a consumer who doesn't set it. Each
component tunes its own padding/font-size per step rather than pulling from one
shared token (see `Blazor/CLAUDE.md`'s "Themes" section on why spacing is
hand-tuned per component) — `FaSize` is the shared *scale*, not shared literal
values. `FaInput`/`FaSelect`/`FaTextarea`/`FaCurrency` do share one CSS ladder
between themselves (`.fa-input-xs`/`-sm`/`-lg`/`-xl` in `_inputs.scss`), since all
four already used identical padding/font-size at `Medium` to begin with.

`FaButton.Size` replaces the old `bool Small` parameter (a breaking change —
`Small="true"` becomes `Size="FaSize.Small"`).

## `Responsive`

An opt-in `bool Responsive` parameter on `FaButton`, `FaCard`, `FaInput<TValue>`,
`FaSelect<TValue>`, `FaTextarea`, and `FaCurrency` — stretches the element to 100%
width below the existing 720px breakpoint (`_responsive.scss`, the same one
`FaSidebarShell`'s off-canvas sidebar uses):

```razor
<FaButton Variant="FaButtonVariant.Primary" Responsive="true">Submit</FaButton>
```

Off by default: a button/card/input's intrinsic width is usually exactly what's
wanted even on small screens, so this is opt-in per instance rather than an
automatic breakpoint change that would silently resize every existing consumer's
markup. All six share one CSS utility class (`.fa-responsive`) rather than each
defining its own identical rule.

## Floating label (`FaInput` only)

`FaInput<TValue>.FloatingLabel` (`bool`, default `false`) renders `Label` as a
Material-style label overlaid inside the input — centered over the placeholder area
until focused or filled, then animating up to sit on the border — instead of a
block label above the field:

```razor
<FaInput @bind-Value="_email" Label="Email" FloatingLabel="true" />
```

Pure CSS (`.fa-field-floating`/`.fa-label-floating` in `_inputs.scss`), no JS —
driven by `:focus`/`:placeholder-shown`. Ignored (falls back to the normal block
label) when `Label` is null/empty. Scoped to `FaInput` today; `FaSelect`/
`FaTextarea`/`FaCurrency` keep their normal block label.

## Input style (standard / minimal / maximal)

A third, app-wide axis — independent of `Size` above, and independent of
[palette](palette-switcher.md)/[theme mode](theme-switcher.md) — that retunes every
boxed native-input-like control (`FaInput`, `FaSelect`, `FaTextarea`, `FaCurrency`)
at once: **Standard** (default, today's look), **Minimal** (1px border, square
corners, no shadow), or **Maximal** (3px border, larger radius). See
[FaInputStyleSwitcher](input-style-switcher.md) for how a consumer sets it.

[← Back to index](index.md)
