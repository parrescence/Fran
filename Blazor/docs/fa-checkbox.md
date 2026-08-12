[← Back to index](index.md)

# FaCheckbox

Checkbox with an optional clickable label (wrapping input + label text in a single
`<label>` gets click-to-toggle for free from the browser). `InputCheckbox`-derived,
so it only works inside an `<EditForm>`.

## Usage

```razor
<EditForm Model="_model">
    <FaCheckbox @bind-Value="_model.IsRecurring" Label="Recurring" />
</EditForm>

@code {
    private class ItemModel
    {
        public bool IsRecurring { get; set; }
    }

    private ItemModel _model = new();
}
```

## Getting the value

`@bind-Value` writes `_model.IsRecurring` on every change.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `bool` | `@bind-Value` |
| `Label` | `string?` | clickable label text |
| `ReadOnly` | `bool` | flattens to a plain ☑/☐ + label with a bottom border instead of a clickable checkbox |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
