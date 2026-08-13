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
    <ChildContent>
        <p>Are you sure you want to delete this item? This can't be undone.</p>
    </ChildContent>

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

**When you set `FooterContent`, wrap the body in `<ChildContent>` too** (as above) —
Razor only maps bare/unwrapped markup to a component's default `ChildContent`
automatically when that's the *only* `RenderFragment` parameter in use at that call
site. `FaModal` has two (`ChildContent` and `FooterContent`), so the moment
`FooterContent` shows up, the body needs its own explicit `<ChildContent>` tag or the
compiler rejects it (`RZ9996`). A modal with no `FooterContent` doesn't need this —
bare content maps to `ChildContent` fine on its own.

## Position, size, and closing

```razor
<FaModal Show="_show" Title="Filters" OnClose="() => _show = false"
         Position="FaModalPosition.Right" Size="FaModalSize.Small"
         ShowCloseButton="false" CloseOnBackdropClick="false">
    <ChildContent>
        ...
    </ChildContent>
    <FooterContent>
        <FaButton OnClick="() => _show = false">Close</FaButton>
    </FooterContent>
</FaModal>
```

`ShowCloseButton="false"` + `CloseOnBackdropClick="false"` together means the only
way out is whatever explicit action you put in `FooterContent` — useful for a form
the user shouldn't be able to dismiss by an accidental outside click or the header ×.

`FooterAlign` controls the footer button row's `justify-content` — `End` (the
default, right-aligned) reads as the usual "Cancel / Confirm" placement; `Between`
puts a destructive action on the left and Cancel/Confirm on the right without a
manual spacer.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Show` | `bool` | caller-owned visibility |
| `Title` | `string?` | header text |
| `OnClose` | `EventCallback` | backdrop click or close button |
| `ChildContent` | `RenderFragment?` | body |
| `FooterContent` | `RenderFragment?` | optional footer (buttons, usually) |
| `Position` | `FaModalPosition` | `Center` (default) \| `Top` \| `Bottom` \| `Left` \| `Right` |
| `Size` | `FaModalSize` | `Small` \| `Medium` (default) \| `Large` |
| `FooterAlign` | `FaAlign` | `Start` \| `Center` \| `End` (default) \| `Between` \| `Around` \| `Evenly` |
| `ShowCloseButton` | `bool` | header × button, defaults to `true` |
| `CloseOnBackdropClick` | `bool` | defaults to `true` |
| `ResetOnClose` | `bool` | tear down/recreate ChildContent on every open instead of just hiding it, defaults to `false` |

[← Back to index](index.md)
