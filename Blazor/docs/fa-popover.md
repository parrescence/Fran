[← Back to index](index.md)

# FaPopover

Click-to-open contextual panel — a menu, a mini-form, extra detail that doesn't
belong inline. Same open/close pattern as `FaDatePicker`'s calendar popup: a plain
Blazor bool, no JS interop, closing on Escape or on focus leaving the control (with
a short grace period so tabbing between elements inside the panel doesn't flicker
it shut).

## Usage

```razor
<FaPopover Position="FaTooltipPosition.Bottom">
    <Trigger>Filters</Trigger>
    <ChildContent>
        <FaCheckbox Label="In stock only" @bind-Value="_inStockOnly" />
        <FaCheckbox Label="On sale" @bind-Value="_onSaleOnly" />
    </ChildContent>
</FaPopover>
```

## Getting the value

No bound value of its own — open/closed state lives inside the component. Whatever
you put in `ChildContent` (checkboxes, inputs, ...) binds the normal way; `FaPopover`
just controls when that content is visible.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Trigger` | `RenderFragment` | **required** — the button's content |
| `ChildContent` | `RenderFragment` | **required** — the panel's content |
| `Position` | `FaTooltipPosition` | `Bottom` (default) \| `Top` \| `Left` \| `Right` — shared enum with `FaTooltip` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
