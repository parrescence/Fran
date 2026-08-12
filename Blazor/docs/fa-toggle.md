[← Back to index](index.md)

# FaToggle&lt;TValue&gt;

Segmented N-option switch — pass 2+ `(string Title, TValue Value)` options, plain
`System.ValueTuple` (not a custom DTO), same shape `FaRadioGroup` uses. Supports an
optional companion input that only shows while the current `Value` satisfies
`ShowInputWhen` (e.g. an amount field that only appears once "Custom" is picked).

## Usage

```razor
<FaToggle TValue="string"
          Label="Billing period"
          Options="_periodOptions"
          @bind-Value="_period" />

@code {
    private string _period = "monthly";

    private IReadOnlyList<(string Title, string Value)> _periodOptions = new[]
    {
        ("Monthly", "monthly"),
        ("Yearly", "yearly")
    };
}
```

### With a companion input

```razor
<FaToggle TValue="string"
          Label="Amount"
          Options="_amountOptions"
          @bind-Value="_amountMode"
          InputType="number"
          InputPlaceholder="Enter amount"
          ShowInputWhen="@(mode => mode == "custom")"
          @bind-InputValue="_customAmountText" />

@code {
    private string _amountMode = "default";
    private string? _customAmountText;

    private IReadOnlyList<(string Title, string Value)> _amountOptions = new[]
    {
        ("Default", "default"),
        ("Custom", "custom")
    };
}
```

## Getting the value

`@bind-Value` keeps `_period`/`_amountMode` in sync as options are clicked (or
navigated with the arrow keys). The companion input's text is separately available
via `@bind-InputValue`.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Options` | `IReadOnlyList<(string Title, TValue Value)>` | **required**, at least 2 |
| `Value` / `ValueChanged` | `TValue` | `@bind-Value` |
| `InputType` | `string?` | e.g. `"text"`, `"number"` — null renders no companion input |
| `InputPosition` | `FaTogglePosition` | `Before` \| `After` the switch |
| `ShowInputWhen` | `Func<TValue, bool>?` | when the companion input is visible |
| `InputValue` / `InputValueChanged` | `string?` | `@bind-InputValue` |
| `Disabled` | `bool` | |
| `ReadOnly` | `bool` | flattens the pill row to the active option's title as plain text with a bottom border |

[← Back to index](index.md)
