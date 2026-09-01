[← Back to index](index.md)

# FaTooltip

Hover/focus label for whatever's wrapped in `ChildContent` — pure CSS (`:hover`,
`:focus-within`), no JS positioning logic, so it never measures anything or reacts
to scroll/resize. `Position` is a hint, not a smart auto-flip: pick the side that
actually has room in your layout.

Keyboard accessibility rides on `:focus-within` rather than a forced `tabindex` on
the wrapper, so it shows correctly when `ChildContent` is itself focusable (a
button, a link) without adding a second, redundant tab stop. Wrapping plain
non-focusable content (bare text, an icon with no button around it) means the
tooltip is mouse/hover-only, same as the native `title` attribute would be.

## Usage

```razor
<FaTooltip Text="Exports the current view as a CSV file">
    <FaButton Variant="FaButtonVariant.Secondary" OnClick="ExportAsync">Export</FaButton>
</FaTooltip>
```

## Getting the value

No bound value — plain display.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Text` | `string` | **required** |
| `Position` | `FaTooltipPosition` | `Top` (default) \| `Bottom` \| `Left` \| `Right` |
| `ChildContent` | `RenderFragment` | **required** — the element the tooltip attaches to |
| `CssClass` | `string?` | |

[← Back to index](index.md)
