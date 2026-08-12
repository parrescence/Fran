[← Back to index](index.md)

# FaInput&lt;TValue&gt;

Generic text/number field. `InputBase<TValue>`-derived, so it only works inside an
`<EditForm>`.

## Usage

```razor
<EditForm Model="_model" OnValidSubmit="SaveAsync">
    <FaInput TValue="string" @bind-Value="_model.Name" Label="Name" Placeholder="Jane Doe" />
    <FaInput TValue="decimal" @bind-Value="_model.Amount" Type="number" Step="0.01" Label="Amount" />

    <FaButton Type="submit">Save</FaButton>
</EditForm>

@code {
    private class ItemModel
    {
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
    }

    private ItemModel _model = new();

    private Task SaveAsync()
    {
        // _model.Name / _model.Amount are already populated
        return Task.CompletedTask;
    }
}
```

## Getting the value

`@bind-Value` writes straight into `_model.Name`/`_model.Amount` on every input event
— read it from the model field at any time, not just in `OnValidSubmit`.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `TValue` | `@bind-Value`, via `EditContext` |
| `Label` | `string?` | |
| `Placeholder` | `string?` | |
| `Type` | `string` | HTML `type`, default `"text"` |
| `Step` | `string?` | for `type="number"` |
| `ReadOnly` | `bool` | same boxed look, muted and non-interactive |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
