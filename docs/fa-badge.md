[← Back to index](index.md)

# FaBadge

Small pill label — invite status, budget over/under, etc.

## Usage

```razor
<FaBadge Variant="@(tx.IsPaid ? FaBadgeVariant.Success : FaBadgeVariant.Danger)">
    @(tx.IsPaid ? "Paid" : "Overdue")
</FaBadge>
```

## Getting the value

No bound value — plain display. Feed its `Variant`/content from whatever data you
already have, as above.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Variant` | `FaBadgeVariant` | `Neutral` (default) \| `Primary` \| `Success` \| `Danger` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
