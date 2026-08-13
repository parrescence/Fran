[← Back to index](index.md)

# FaForm&lt;TModel&gt;

A self-contained wrapper around `EditForm`: hand it a model and your own field
markup — ordinary `FaInput`/`FaSelect`/etc. with `@bind-Value` straight to the
model, written normally in your own `.razor` file — and it hands the populated
model back via `OnSubmit` once validation passes. FaForm doesn't generate fields
itself; it supplies the `EditContext` your fields bind against, plus the
boilerplate every hand-rolled `EditForm` otherwise repeats: `DataAnnotationsValidator`
+ `ValidationSummary`, and a Cancel/Delete/Submit button row.

There's no separate "new form"/"update form" component — `Mode` is what tells one
`FaForm<TModel>` apart from the other; it only changes the default submit-button
text ("Create" vs "Save") and whether Delete shows.

## Usage

```razor
<FaForm TModel="ProductModel" Model="_model" Mode="_mode"
        OnSubmit="SaveAsync" OnCancel="() => _showModal = false" OnDelete="DeleteAsync">
    <FaInput TValue="string" @bind-Value="_model.Name" Label="Name" />
    <FaInput TValue="decimal" @bind-Value="_model.Price" Type="number" Step="0.01" Label="Price" />
</FaForm>

@code {
    private ProductModel _model = new();
    private FaFormMode _mode = FaFormMode.Create;

    private async Task SaveAsync(ProductModel model)
    {
        // model.Name / model.Price are already populated and passed validation
        await _productService.SaveAsync(model);
        _showModal = false;
    }

    private async Task DeleteAsync()
    {
        await _productService.DeleteAsync(_model.Id);
        _showModal = false;
    }
}
```

Drop it inside an [FaModal](fa-modal.md) for the common "create/edit in a dialog"
placement, or render it inline on a page.

## Getting the value

`OnSubmit` hands back the same `TModel` instance you passed in as `Model` — already
populated, already past validation. There's no separate `@bind`; the fields inside
`ChildContent` write straight into `Model`'s properties the normal `@bind-Value` way.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Model` | `TModel` | required |
| `ChildContent` | `RenderFragment?` | your own field markup |
| `OnSubmit` | `EventCallback<TModel>` | fires after validation passes |
| `OnCancel` | `EventCallback` | Cancel button only shows if this has a delegate |
| `OnDelete` | `EventCallback` | Delete button only shows if this has a delegate **and** `Mode` is `Edit` |
| `Mode` | `FaFormMode` | `Create` (default) \| `Edit` |
| `SubmitText` | `string?` | overrides the Mode-derived default ("Create"/"Save") |
| `CancelText` | `string` | defaults to `"Cancel"` |
| `DeleteText` | `string` | defaults to `"Delete"` |
| `Busy` | `bool` | disables Submit and swaps its text to "Saving…" — flip this around your own `OnSubmit` await |
| `ShowValidationSummary` | `bool` | defaults to `true` |
| `ButtonAlign` | `FaAlign` | `Start` \| `Center` \| `End` (default) \| `Between` \| `Around` \| `Evenly` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
