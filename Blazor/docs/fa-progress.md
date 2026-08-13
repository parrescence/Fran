[← Back to index](index.md)

# FaProgress

A filled track reporting how far along something is — a budget spent, an upload's
percent complete, a wizard step. `Direction` picks which edge the fill grows from:
`Right`/`Left` are horizontal, `Up`/`Down` are vertical.

## Usage

```razor
<FaProgress Value="@_uploadedBytes" Max="@_totalBytes" />

<FaProgress Value="65" Direction="FaProgressDirection.Up" />

<FaProgress Value="@_budgetSpent" Max="@_budgetLimit"
            Variant="@(_budgetSpent > _budgetLimit ? FaLoaderVariant.Danger : FaLoaderVariant.Accent)" />
```

## Getting the value

No bound value — it's a plain display of whatever `Value`/`Max` you pass in. Update
`Value` from your own state (an upload progress callback, a step counter, ...) and
the fill's width/height transitions to match.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` | `double` | |
| `Max` | `double` | defaults to `100` |
| `Direction` | `FaProgressDirection` | `Right` (default) \| `Left` \| `Up` \| `Down` |
| `Variant` | `FaLoaderVariant` | `Accent` (default) \| `Primary` \| `Gold` \| `Danger` — shared with FaSpinner/FaHelixLoader/FaPongLoader |
| `CssClass` | `string?` | vertical tracks (`Up`/`Down`) default to 120px tall — override via `CssClass` for a different size |

[← Back to index](index.md)
