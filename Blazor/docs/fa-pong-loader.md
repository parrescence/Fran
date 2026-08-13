[← Back to index](index.md)

# FaPongLoader

A miniature game of Pong as a loading indicator — a ball bounces around the court
while the two paddles bob up and down, out of phase with each other.

## Usage

```razor
<FaPongLoader />

<FaPongLoader Variant="FaLoaderVariant.Gold" />
```

## Getting the value

Purely presentational — nothing to read back. Show/hide it with your own `@if`
around whatever loading flag you're already tracking.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Variant` | `FaLoaderVariant` | `Accent` (default) \| `Primary` \| `Gold` \| `Danger` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
