[← Back to index](index.md)

# FaModal

Backdrop-covered dialog. **You own the open/closed state** — `FaModal` has no
internal show/hide, it just renders nothing when `Show` is `false`. That means you
can gate closing behind your own logic (e.g. an unsaved-changes confirm) before
flipping `Show` back to `false`.

## Usage

```razor
<FaButton OnClick="() => _showDeleteModal = true">Delete item</FaButton>

<FaModal Show="_showDeleteModal" Title="Confirm delete" OnClose="() => _showDeleteModal = false">
    <p>Are you sure you want to delete this item? This can't be undone.</p>

    <FooterContent>
        <FaButton Variant="FaButtonVariant.Secondary" OnClick="() => _showDeleteModal = false">
            Cancel
        </FaButton>
        <FaButton Variant="FaButtonVariant.Danger" OnClick="ConfirmDeleteAsync">
            Delete
        </FaButton>
    </FooterContent>
</FaModal>

@code {
    private bool _showDeleteModal;

    private async Task ConfirmDeleteAsync(MouseEventArgs e)
    {
        await _itemService.DeleteAsync(_itemId);
        _showDeleteModal = false;
    }
}
```

## Getting the value

No bound value — it's a container. `OnClose` fires when the backdrop or the header's
close button is clicked; it's up to you to flip `Show` back to `false` in response
(or not, if you want to block the close).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Show` | `bool` | caller-owned visibility |
| `Title` | `string?` | header text |
| `OnClose` | `EventCallback` | backdrop click or close button |
| `ChildContent` | `RenderFragment?` | body |
| `FooterContent` | `RenderFragment?` | optional footer (buttons, usually) |

[← Back to index](index.md)
