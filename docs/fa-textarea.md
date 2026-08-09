[← Back to index](index.md)

# FaTextarea

Multi-line text field. `InputBase`-derived (via `InputTextArea`), so it only works
inside an `<EditForm>`. Set `MaxLength` for a live character counter, or
`ReadOnlyDisplay` to render the value as plain flowed text instead of a boxed,
scrollable textarea.

## Usage

```razor
<EditForm Model="_model">
    <FaTextarea @bind-Value="_model.Notes" Label="Notes" MaxLength="280" />
</EditForm>

@code {
    private class TransactionModel
    {
        public string? Notes { get; set; }
    }

    private TransactionModel _model = new();
}
```

## Getting the value

`@bind-Value` writes into `_model.Notes` on every input event.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `string?` | `@bind-Value` |
| `Label` | `string?` | |
| `Placeholder` | `string?` | |
| `MaxLength` | `int?` | shows a live counter when set |
| `ReadOnlyDisplay` | `bool` | render as plain text instead of an editable box |

[← Back to index](index.md)
