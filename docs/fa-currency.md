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
    private class TransactionModel
    {
        public decimal? Amount { get; set; }
    }

    private TransactionModel _model = new();
}
```

## Getting the value

`@bind-Value` writes into `_model.Amount` — values are rounded to 2 decimal places
and clamped to `Min`/`Max` as they're typed.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `decimal?` | `@bind-Value` |
| `Label` | `string?` | |
| `Min` / `Max` | `decimal?` | |
| `CurrencySymbol` | `string` | default `"$"` |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
