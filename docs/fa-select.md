[← Back to index](index.md)

# FaSelect&lt;TValue&gt;

Plain `<select>`, re-skinned — pass `<option>` elements as child content, same as a
native select. `InputBase<TValue>`-derived, so it only works inside an `<EditForm>`.

## Usage

```razor
<EditForm Model="_model">
    <FaSelect TValue="string" @bind-Value="_model.Category" Label="Category">
        <option value="">Choose…</option>
        <option value="groceries">Groceries</option>
        <option value="rent">Rent</option>
        <option value="utilities">Utilities</option>
    </FaSelect>
</EditForm>

@code {
    private class FilterModel
    {
        public string Category { get; set; } = "";
    }

    private FilterModel _model = new();
}
```

## Getting the value

`@bind-Value` keeps `_model.Category` in sync with the selected `<option value>` on
every change.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `TValue` | `@bind-Value` |
| `Label` | `string?` | |
| `ChildContent` | `RenderFragment?` | `<option>` elements |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
