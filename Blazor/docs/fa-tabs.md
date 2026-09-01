[← Back to index](index.md)

# FaTabs&lt;TValue&gt;

Tab strip — pass 2+ `(string Title, TValue Value)` options, same tuple shape as
`FaToggle`/`FaRadioGroup`, but rendered as a proper `role="tablist"`/`role="tab"`
strip with an underline active-indicator instead of a segmented pill row. FaTabs
only renders the strip itself — which panel shows is your own `@if` on whichever
value you bind, same "caller owns the content" shape `FaDropdown`/`FaSearchSelect`
already use for their own selection.

## Usage

```razor
<FaTabs TValue="string" Options="_tabs" @bind-Value="_activeTab" />

@if (_activeTab == "details")
{
    <p>Order details go here.</p>
}
else if (_activeTab == "history")
{
    <p>Order history goes here.</p>
}

@code {
    private string _activeTab = "details";

    private IReadOnlyList<(string Title, string Value)> _tabs = new[]
    {
        ("Details", "details"),
        ("History", "history")
    };
}
```

## Getting the value

`@bind-Value` keeps `_activeTab` in sync as tabs are clicked or navigated with the
arrow keys (Home/End jump to the first/last tab, same as a native tablist).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Options` | `IReadOnlyList<(string Title, TValue Value)>` | **required**, at least 2 |
| `Value` / `ValueChanged` | `TValue` | `@bind-Value` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
