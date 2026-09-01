[← Back to index](index.md)

# FaSelect&lt;TValue&gt;

Plain `<select>`, re-skinned — pass `<option>` elements as child content, same as a
native select. `InputBase<TValue>`-derived, so it only works inside an `<EditForm>`.

## Usage

```razor
<EditForm Model="_model">
    <FaSelect TValue="string" @bind-Value="_model.Type" Label="Type">
        <option value="">Choose…</option>
        <option value="general">General</option>
        <option value="urgent">Urgent</option>
        <option value="archived">Archived</option>
    </FaSelect>
</EditForm>

@code {
    private class FilterModel
    {
        public string Type { get; set; } = "";
    }

    private FilterModel _model = new();
}
```

## Getting the value

`@bind-Value` keeps `_model.Type` in sync with the selected `<option value>` on
every change.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `TValue` | `@bind-Value` |
| `Label` | `string?` | |
| `ChildContent` | `RenderFragment?` | `<option>` elements |
| `ReadOnly` | `bool` | renders `disabled` (no native `readonly` on `<select>`), same boxed look, muted |
| `ShowValidationMessage` | `bool` | defaults `true` — see [Validation](validation.md) |
| `Validate` | `Func<TValue, string?>?` | element-tier validation override — see [Validation](validation.md) |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
