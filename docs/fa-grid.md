[← Back to index](index.md)

# FaGrid&lt;TItem&gt;

A step up from [FaTable](fa-table.md): owns paging/rows-per-page and per-column
sorting/filtering itself. Works in exactly one of two modes, picked by which
parameter you set — mixing both or setting neither throws at runtime.

- **`Items`** — the whole dataset in memory; FaGrid filters/sorts/pages it
  client-side. Simplest option, fine for small-to-medium lists.
- **`ItemsProvider`** — FaGrid never holds more than one page in memory. Every page
  change, page-size change, header-sort click, or filter change builds an
  `FaGridRequest` (page index/size, sort key, selected filters) and awaits your
  function; you return just that page's rows plus a total count as an
  `FaGridResult<TItem>`. Use this when the data comes from a real paged query/API and
  you don't want (or can't fit) the whole thing in memory.

## Usage — `Items` (in-memory)

```razor
<FaGrid TItem="Transaction" Items="_transactions" Columns="_columns" PageSize="10" />

@code {
    private List<Transaction> _transactions = new(); // your full, unpaged dataset

    private IReadOnlyList<FaGridColumn<Transaction>> _columns = new[]
    {
        new FaGridColumn<Transaction>
        {
            Header = "Date",
            Sortable = true,
            SortKey = tx => tx.Date,
            CellContent = tx => @<span>@tx.Date.ToString("MMM d")</span>
        },
        new FaGridColumn<Transaction>
        {
            Header = "Name",
            CellContent = tx => @<span>@tx.Name</span>
        },
        new FaGridColumn<Transaction>
        {
            Header = "Amount",
            Sortable = true,
            SortKey = tx => tx.Amount,
            CellContent = tx => @<span>@tx.Amount.ToString("C")</span>
        },
        new FaGridColumn<Transaction>
        {
            Header = "Status",
            FilterOptions = new[] { ("paid", "Paid"), ("overdue", "Overdue") },
            FilterPredicate = (tx, key) => key == "paid" ? tx.IsPaid : !tx.IsPaid,
            CellContent = tx => @<FaBadge Variant="@(tx.IsPaid ? FaBadgeVariant.Success : FaBadgeVariant.Danger)">
                                     @(tx.IsPaid ? "Paid" : "Overdue")
                                 </FaBadge>
        }
    };
}
```

## Usage — `ItemsProvider` (paged query)

```razor
<FaGrid TItem="Transaction" ItemsProvider="LoadPageAsync" Columns="_columns" PageSize="25" />

@code {
    private async Task<FaGridResult<Transaction>> LoadPageAsync(FaGridRequest request)
    {
        // request.PageIndex, request.PageSize, request.SortKey, request.SortAscending,
        // request.Filters (columnKey -> selected filter key) all come from the grid —
        // push them into your real query.
        var (rows, total) = await _transactionService.QueryAsync(
            skip: request.PageIndex * request.PageSize,
            take: request.PageSize,
            sortBy: request.SortKey,
            ascending: request.SortAscending,
            statusFilter: request.Filters.GetValueOrDefault("Status"));

        return new FaGridResult<Transaction>
        {
            Items = rows,       // just this page
            TotalCount = total  // across every page
        };
    }
}
```

## Getting the value

`FaGrid` doesn't hand back a "selected row" — like `FaTable`, put your own click
handler inside a column's `CellContent` if you need row selection/navigation. What it
does manage for you is paging/sort/filter *state*: which page you're on, the current
sort column/direction, and the selected filter per column all live inside the
component. `PageSize` is two-way bindable (`@bind-PageSize`) if you want to know or
set the current rows-per-page from outside.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Items` | `IReadOnlyList<TItem>?` | whole dataset — exactly one of this or `ItemsProvider` |
| `ItemsProvider` | `Func<FaGridRequest, Task<FaGridResult<TItem>>>?` | paged fetch — exactly one of this or `Items` |
| `Columns` | `IReadOnlyList<FaGridColumn<TItem>>` | **required** |
| `PageSize` / `PageSizeChanged` | `int` | default `10`, `@bind-PageSize` |
| `PageSizeOptions` | `IReadOnlyList<int>` | default `10, 25, 50, 100` |
| `EmptyText` | `string?` | shown when there are no rows |
| `CssClass` | `string?` | |

### `FaGridColumn<TItem>`

| Property | Type | Notes |
|---|---|---|
| `Header` | `string` | **required** |
| `CellContent` | `Func<TItem, RenderFragment>` | **required** |
| `Key` | `string?` | identifies the column in `FaGridRequest`; defaults to `Header` |
| `Sortable` | `bool` | |
| `SortKey` | `Func<TItem, IComparable>?` | **required if `Sortable`** in `Items` mode; unused in provider mode |
| `FilterOptions` | `IReadOnlyList<(string Key, string Label)>?` | "All" is added automatically |
| `FilterPredicate` | `Func<TItem, string, bool>?` | **required if `FilterOptions` is set** in `Items` mode; unused in provider mode |
| `CssClass` | `string?` | |

[← Back to index](index.md)
