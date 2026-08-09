[← Back to index](index.md)

# FaSearchSelect&lt;TItem&gt;

Type-to-search combobox. You supply the lookup (`QueryAsync`) — an in-memory filter,
an API call, whatever — FaSearchSelect debounces keystrokes, shows the results in a
dropdown, and commits `Value` when one is picked. Not `InputBase`-based (the bound
value is an arbitrary `TItem`, not a string), so it works with or without an
`EditForm` — plain `@bind-Value` like `FaToggle`.

## Usage

```razor
<FaSearchSelect TItem="Payer"
                Label="Payer"
                Placeholder="Type a name…"
                QueryAsync="SearchPayersAsync"
                ItemLabel="p => p.Name"
                @bind-Value="_selectedPayer" />

@code {
    private Payer? _selectedPayer;

    private async Task<IReadOnlyList<Payer>> SearchPayersAsync(string query)
    {
        return await _payerService.SearchAsync(query); // your own lookup
    }
}
```

### Custom row rendering

```razor
<FaSearchSelect TItem="Payer"
                QueryAsync="SearchPayersAsync"
                ItemLabel="p => p.Name"
                @bind-Value="_selectedPayer">
    <ItemTemplate Context="payer">
        <strong>@payer.Name</strong> <span class="fa-text-muted">@payer.Email</span>
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

[← Back to index](index.md)
