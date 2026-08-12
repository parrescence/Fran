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
        foreach (var order in _orders)
        {
            _rows.Add(new Dictionary<string, RenderFragment>
            {
                ["Name"] = @<span>@order.Name</span>,
                ["Amount"] = @<span>@order.Amount.ToString("C")</span>,
                ["Status"] = @<FaBadge Variant="@(order.IsPaid ? FaBadgeVariant.Success : FaBadgeVariant.Danger)">
                                 @(order.IsPaid ? "Paid" : "Overdue")
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

## Responsive card mode (≤720px)

Below the same 720px breakpoint the sidebar collapses at, `.fa-table` switches from a
grid table to a stack of bordered cards — one per row, `<thead>` hidden (visually, not
`display:none`, so it stays screen-reader accessible) and each `<td>` printing a label
next to its value.

The label comes from each `<td>`'s `data-label` attribute. `FaTable` emits it
automatically from `Columns` — nothing to change in your markup. Hand-written
`<table class="fa-table">` markup opts in by adding `data-label` itself:

```html
<table class="fa-table">
  <thead>
    <tr><th>Name</th><th>Amount</th><th class="fa-table-actions">Actions</th></tr>
  </thead>
  <tbody>
    <tr>
      <td data-label="Name">Order #1024</td>
      <td data-label="Amount">$1,200.00</td>
      <td class="fa-table-actions">
        <button class="fa-btn fa-btn-sm">Edit</button>
      </td>
    </tr>
  </tbody>
</table>
```

`.fa-table-actions` marks the action column — on desktop it shrinks to fit its content
(`width:1%; white-space:nowrap`, replacing the inline-style trick consumers previously
hand-rolled); in card mode it drops its label and renders as its own right-aligned row
pinned above the data fields, assumed to be the last `<td>` in each row.

[← Back to index](index.md)
