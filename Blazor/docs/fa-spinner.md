[← Back to index](index.md)

# FaSpinner

The classic rotating-ring spinner — a button's busy state, a panel waiting on data,
anywhere a compact "working on it" indicator is enough on its own.

## Usage

```razor
<FaSpinner />

<FaSpinner Size="16px" Variant="FaLoaderVariant.Primary" />
```

## Getting the value

Purely presentational — nothing to read back. Show/hide it with your own `@if`
around whatever loading flag you're already tracking.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Size` | `string` | any CSS size, defaults to `"24px"` |
| `Variant` | `FaLoaderVariant` | `Accent` (default) \| `Primary` \| `Gold` \| `Danger` |
| `Label` | `string` | `aria-label`, defaults to `"Loading"` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
