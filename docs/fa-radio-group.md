[← Back to index](index.md)

# FaRadioGroup&lt;TValue&gt;

A group of radio buttons — pass 1+ `(string Title, TValue Value)` options, same shape
as `FaToggle`'s `Options`. Not `InputBase`-based — plain `@bind-Value`, works with or
without an `EditForm`.

## Usage

```razor
<FaRadioGroup TValue="string"
              Label="Frequency"
              Options="_frequencyOptions"
              @bind-Value="_model.Frequency" />

@code {
    private class BudgetModel
    {
        public string Frequency { get; set; } = "monthly";
    }

    private BudgetModel _model = new();

    private IReadOnlyList<(string Title, string Value)> _frequencyOptions = new[]
    {
        ("Weekly", "weekly"),
        ("Monthly", "monthly"),
        ("Yearly", "yearly")
    };
}
```

## Getting the value

`@bind-Value` keeps `_model.Frequency` in sync as options are clicked.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Options` | `IReadOnlyList<(string Title, TValue Value)>` | **required** |
| `Value` / `ValueChanged` | `TValue?` | `@bind-Value` |
| `Label` | `string?` | |
| `Disabled` | `bool` | |

[← Back to index](index.md)
