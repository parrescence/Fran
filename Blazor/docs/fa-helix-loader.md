[← Back to index](index.md)

# FaHelixLoader

A DNA-helix-style loader: a row of "rungs", each a top/bottom dot pair that swings
in opposite phase (the twist), with a staggered per-rung delay sending that twist
traveling down the row while the whole loader breathes in and out.

## Usage

```razor
<FaHelixLoader />

<FaHelixLoader RungCount="3" Variant="FaLoaderVariant.Primary" />
```

## Getting the value

Purely presentational — nothing to read back. Show/hide it with your own `@if`
around whatever loading flag you're already tracking.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `RungCount` | `int` | defaults to `5`, clamped to `1`–`6` |
| `Variant` | `FaLoaderVariant` | `Accent` (default) \| `Primary` \| `Gold` \| `Danger` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
