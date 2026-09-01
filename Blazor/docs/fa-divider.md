[← Back to index](index.md)

# FaDivider

A section separator — a plain rule, or a rule split around a short label (e.g. "OR"
between two sign-in options). `Vertical` divides side-by-side content instead of
stacked content; `Label` is ignored when `Vertical` is true.

## Usage

```razor
<FaDivider />

<FaDivider Label="OR" />

<div class="fa-flex fa-align-center">
    <span>Left</span>
    <FaDivider Vertical="true" />
    <span>Right</span>
</div>
```

## Getting the value

No bound value — plain display.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Label` | `string?` | ignored when `Vertical` is true |
| `Vertical` | `bool` | |
| `CssClass` | `string?` | |

[← Back to index](index.md)
