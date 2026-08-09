[← Back to index](index.md)

# FaTable

Renders a real `<table>`/`<thead>`/`<tbody>` from typed data — pass `Columns`
(header text, defines column order) and `Rows` (one dictionary per row, keyed by
column name, each value a `RenderFragment` so a cell can hold arbitrary content).
Plain display — no paging/sorting/filtering; see [FaGrid](fa-grid.md) if you need
those.

## Usage

```razor
<FaTable Columns="_columns" Rows="_rows" />

@code {
    private List<string> _columns = new() { "Name", "Amount", "Status" };
    private List<IReadOnlyDictionary<string, RenderFragment>> _rows = new();

    protected override void OnInitialized()
    {
        foreach (var tx in _transactions)
        {
            _rows.Add(new Dictionary<string, RenderFragment>
            {
                ["Name"] = @<span>@tx.Name</span>,
                ["Amount"] = @<span>@tx.Amount.ToString("C")</span>,
                ["Status"] = @<FaBadge Variant="@(tx.IsPaid ? FaBadgeVariant.Success : FaBadgeVariant.Danger)">
                                 @(tx.IsPaid ? "Paid" : "Overdue")
                             </FaBadge>
            });
        }
    }
}
```

## Getting the value

There's no selection/value to read back — `FaTable` is pure display. If you need row
click/selection, wrap each cell's content in your own click handler (e.g. put an
`FaCard` or a `<button>` inside the `RenderFragment`).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Columns` | `IReadOnlyList<string>` | **required** — header text, defines column order |
| `Rows` | `IReadOnlyList<IReadOnlyDictionary<string, RenderFragment>>` | **required** — one dict per row, keyed by column name |
| `CssClass` | `string?` | |

[← Back to index](index.md)
