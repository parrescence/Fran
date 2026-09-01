[← Back to index](index.md)

# FaCurrency

Numeric amount field that shows a formatted `$ 12.34` while unfocused and a plain
editable number while focused. `InputBase<decimal?>`-derived, so it only works
inside an `<EditForm>`. No JS interop.

## Usage

```razor
<EditForm Model="_model">
    <FaCurrency @bind-Value="_model.Amount" Label="Amount" Min="0" />
</EditForm>

@code {
    private class ItemModel
    {
        public decimal? Amount { get; set; }
    }

    private ItemModel _model = new();
}
```

## Getting the value

`@bind-Value` writes into `_model.Amount` — values are rounded to 2 decimal places
and clamped to `Min`/`Max` as they're typed.

## Min/Max validation

If both `Min` and `Max` are set and `Max` ends up below `Min`, FaCurrency shows a
`.fa-validation-message` under the label instead of silently clamping every typed
value the same way. Same non-throwing check as [FaDate's own Min/Max
check](fa-date.md#minmax-validation) — this is FaCurrency checking its own two
parameters directly, not routed through [FaFa's validation system](validation.md).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `decimal?` | `@bind-Value` |
| `Label` | `string?` | |
| `Min` / `Max` | `decimal?` | |
| `CurrencySymbol` | `string` | default `"$"` |
| `ReadOnly` | `bool` | same boxed look, muted and non-interactive |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
