[← Back to index](index.md)

# FaLoadingDots

"Loading" with a trailing ellipsis that grows one dot at a time (`.` → `..` →
`...`) then snaps back and repeats — a plain-text loading indicator for places a
spinner would feel too heavy, e.g. inline in a sentence.

## Usage

```razor
<FaLoadingDots />

<FaLoadingDots Text="Saving" />
```

## Getting the value

Purely presentational — nothing to read back. Show/hide it with your own `@if`
around whatever loading flag you're already tracking.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Text` | `string` | defaults to `"Loading"` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
