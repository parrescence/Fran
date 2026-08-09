[← Back to index](index.md)

# FaDateRange

Linked From/To date fields for range filters (e.g. "show transactions between X and
Y"). Editing one end past the other pushes the other to match instead of rejecting
the edit. Not `InputBase`-based — plain `@bind-Value` of an `FaDateRangeValue`, works
with or without an `EditForm`.

## Usage

```razor
<FaDateRange Label="Date range" @bind-Value="_range" />

<p>Showing transactions from @_range.From?.ToString("MMM d") to @_range.To?.ToString("MMM d")</p>

@code {
    private FaDateRangeValue _range = new(
        DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
        DateOnly.FromDateTime(DateTime.Today));
}
```

## Getting the value

`@bind-Value` gives you one `FaDateRangeValue` with `.From`/`.To` (each `DateOnly?`) —
read both off the same bound field, no need to bind each end separately.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `FaDateRangeValue` | `@bind-Value` — a `(DateOnly? From, DateOnly? To)` record struct |
| `Label` | `string?` | |
| `FromLabel` / `ToLabel` | `string?` | default `"From"`/`"To"` |
| `Min` / `Max` | `DateOnly?` | applies to both ends |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
