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
    private class ItemModel
    {
        public string? Notes { get; set; }
    }

    private ItemModel _model = new();
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
| `ReadOnly` | `bool` | same boxed textarea look, muted and non-interactive |
| `ReadOnlyDisplay` | `bool` | drops the box entirely, flows as plain text instead — wins over `ReadOnly` if both are set |
| `Size` | `FaSize` | `XSmall` \| `Small` \| `Medium` (default) \| `Large` \| `XLarge` — see [Sizing](sizing.md) |
| `Responsive` | `bool` | stretches to 100% width below 720px, default `false` — see [Sizing](sizing.md#responsive) |
| `ShowValidationMessage` | `bool` | defaults `true` — see [Validation](validation.md) |
| `Validate` | `Func<string?, string?>?` | element-tier validation override — see [Validation](validation.md) |

[← Back to index](index.md)
