[← Back to index](index.md)

# FaDropdown&lt;TItem&gt;

Dropdown over either a local `Items` list or a server-paged `QueryPageAsync`
provider, in two selection modes picked via `Searchable`. Either way, it never
shows more than `MaxVisibleItems` rows (default 10) at once: scroll with the mouse
wheel, or arrow past the visible edge with the keyboard, and the window slides —
one row swaps out, the next swaps in — instead of the dropdown growing or scrolling
natively. Not `InputBase`-based, so it works with or without an `EditForm` — plain
`@bind-Value` like `FaToggle`.

- **`Searchable="true"` (default)** — a type-to-filter combobox: the field is
  editable and narrows the results by a contains-text match as you type. When
  opened with an existing selection, it displays the full list with the selected
  item highlighted and scrolled into view, narrowing only when actively typing.
  Dismissing without a selection (clicking outside or pressing Escape) resets the
  input back to the selected item's label.
- **`Searchable="false"`** — a plain dropdown: the field is read-only, clicking it
  toggles the full (windowed) result set open/closed, same interaction shape as a
  native `<select>`.

## Usage

```razor
<FaDropdown TItem="Person"
            Label="Person"
            Placeholder="Type a name…"
            Items="_people"
            ItemLabel="p => p.Name"
            @bind-Value="_selectedPerson" />

@code {
    private Person? _selectedPerson;
    private List<Person> _people = ...; // your own local list
}
```

### Plain dropdown (no search)

```razor
<FaDropdown TItem="Person"
            Label="Person"
            Searchable="false"
            Items="_people"
            ItemLabel="p => p.Name"
            @bind-Value="_selectedPerson" />
```

### Pulling from a database (server paging)

Set `QueryPageAsync` instead of `Items` to back the dropdown with a database/API
call. It's asked for one page at a time — `(search, skip, take)` in, `(items,
totalCount)` out — same "caller owns the data source" split as
[FaSearchSelect](fa-search-select.md)'s `QueryAsync` and
[FaGrid](fa-grid.md)'s `ItemsProvider`. The first page loads when the dropdown
opens (debounced re-query as the search text changes); scrolling past the rows
already loaded pulls the next page automatically — the component only ever asks for
exactly the rows it needs to render.

```razor
<FaDropdown TItem="Person"
            Label="Person"
            Placeholder="Type a name…"
            QueryPageAsync="QueryPeoplePageAsync"
            ItemLabel="p => p.Name"
            @bind-Value="_selectedPerson" />

@code {
    private Person? _selectedPerson;

    private async Task<(IReadOnlyList<Person> Items, int TotalCount)> QueryPeoplePageAsync(string search, int skip, int take)
    {
        return await _peopleService.SearchPageAsync(search, skip, take); // your own paged lookup
    }
}
```

### Custom row rendering

```razor
<FaDropdown TItem="Person"
            Items="_people"
            ItemLabel="p => p.Name"
            @bind-Value="_selectedPerson">
    <ItemTemplate Context="person">
        <strong>@person.Name</strong> <span class="fa-text-muted">@person.Email</span>
    </ItemTemplate>
</FaDropdown>
```

## Getting the value

`@bind-Value` gives you the selected `TItem` directly. `@bind-SearchText` is also
available if you want the raw typed text.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Items` | `IReadOnlyList<TItem>` | the full local list to filter/select from — ignored when `QueryPageAsync` is set |
| `QueryPageAsync` | `Func<string, int, int, Task<(IReadOnlyList<TItem> Items, int TotalCount)>>?` | server-paged alternative to `Items` — `(search, skip, take)` in, `(items, totalCount)` out |
| `ItemLabel` | `Func<TItem, string>` | **required** — text shown for an item; in local mode, also what the search matches against |
| `ItemTemplate` | `RenderFragment<TItem>?` | optional custom row rendering |
| `Value` / `ValueChanged` | `TItem?` | `@bind-Value` |
| `SearchText` / `SearchTextChanged` | `string?` | `@bind-SearchText` |
| `Searchable` | `bool` | default `true` — `false` gives a plain read-only click-to-open dropdown instead of a type-to-filter combobox |
| `ReadOnly` | `bool` | flattens the whole field to the selected item's label as plain text with a bottom border |
| `MaxVisibleItems` | `int` | default `10` — rows shown at once; scrolling/arrow keys slide the window instead of growing past this. Also the page size passed to `QueryPageAsync` |
| `DebounceMilliseconds` | `int` | default `250` — delay before a search-text change re-queries `QueryPageAsync`; ignored in local mode |
| `NoResultsText` | `string` | |
| `LoadingText` | `string` | shown while the first page of a `QueryPageAsync` query is in flight |

[← Back to index](index.md)
