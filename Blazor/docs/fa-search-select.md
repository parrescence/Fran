[← Back to index](index.md)

# FaSearchSelect&lt;TItem&gt;

Type-to-search combobox. You supply the lookup (`QueryAsync`) — an in-memory filter,
an API call, whatever — FaSearchSelect debounces keystrokes, shows the results in a
dropdown, and commits `Value` when one is picked. Not `InputBase`-based (the bound
value is an arbitrary `TItem`, not a string), so it works with or without an
`EditForm` — plain `@bind-Value` like `FaToggle`.

## Usage

```razor
<FaSearchSelect TItem="Person"
                Label="Person"
                Placeholder="Type a name…"
                QueryAsync="SearchPeopleAsync"
                ItemLabel="p => p.Name"
                @bind-Value="_selectedPerson" />

@code {
    private Person? _selectedPerson;

    private async Task<IReadOnlyList<Person>> SearchPeopleAsync(string query)
    {
        return await _peopleService.SearchAsync(query); // your own lookup
    }
}
```

### Custom row rendering

```razor
<FaSearchSelect TItem="Person"
                QueryAsync="SearchPeopleAsync"
                ItemLabel="p => p.Name"
                @bind-Value="_selectedPerson">
    <ItemTemplate Context="person">
        <strong>@person.Name</strong> <span class="fa-text-muted">@person.Email</span>
    </ItemTemplate>
</FaSearchSelect>
```

## Getting the value

`@bind-Value` gives you the selected `TItem` directly — no re-lookup needed.
`@bind-SearchText` is also available if you want the raw typed text (e.g. to show a
"create new" option when nothing matches).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `QueryAsync` | `Func<string, Task<IReadOnlyList<TItem>>>` | **required** |
| `ItemLabel` | `Func<TItem, string>` | **required** — text shown for an item |
| `ItemTemplate` | `RenderFragment<TItem>?` | optional custom row rendering |
| `Value` / `ValueChanged` | `TItem?` | `@bind-Value` |
| `SearchText` / `SearchTextChanged` | `string?` | `@bind-SearchText` |
| `MinQueryLength` | `int` | default `1` — shorter text doesn't query |
| `DebounceMilliseconds` | `int` | default `250` |
| `NoResultsText` | `string` | |
| `ReadOnly` | `bool` | flattens to the selected item's label as plain text with a bottom border |

[← Back to index](index.md)
