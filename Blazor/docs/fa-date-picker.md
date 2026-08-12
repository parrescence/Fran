[← Back to index](index.md)

# FaDatePicker

Split day/month/year date entry with a calendar popup, `Min`/`Max` range support, and
an optional floating label. `InputBase<DateOnly?>`-derived, so it only works inside
an `<EditForm>`. No JS interop — the popup and "close on focus leaving the control"
are plain Blazor state.

## Usage

```razor
<EditForm Model="_model">
    <FaDatePicker @bind-Value="_model.Date"
                  Label="Start date"
                  Max="DateOnly.FromDateTime(DateTime.Today)" />
</EditForm>

@code {
    private class ItemModel
    {
        public DateOnly? Date { get; set; }
    }

    private ItemModel _model = new();
}
```

## Getting the value

`@bind-Value` writes into `_model.Date` — typing all three fields, using the arrow
keys, or picking a day on the calendar all commit through the same path.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `DateOnly?` | `@bind-Value` |
| `Label` | `string?` | |
| `FloatingLabel` | `bool` | label overlaps the field instead of sitting above it |
| `Min` / `Max` | `DateOnly?` | |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
