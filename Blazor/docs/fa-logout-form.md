[← Back to index](index.md)

# FaLogoutForm

A ready-made "are you sure you want to log out?" confirmation — a message and
Confirm/Cancel buttons, nothing to fill in. FaLogoutForm never signs anyone out
itself; `OnConfirm` is where you actually clear the session.

## Usage

```razor
<FaModal Show="_showLogoutConfirm" Title="Log out" OnClose="() => _showLogoutConfirm = false">
    <FaLogoutForm OnConfirm="LogoutAsync" OnCancel="() => _showLogoutConfirm = false" Busy="_busy" />
</FaModal>

@code {
    private bool _showLogoutConfirm;
    private bool _busy;

    private async Task LogoutAsync()
    {
        _busy = true;
        await _authService.LogoutAsync();
        _busy = false;
        _showLogoutConfirm = false;
    }
}
```

Or render it inline — it doesn't have to be inside a modal.

## Getting the value

No bound value — `OnConfirm` is the signal to actually log out, `OnCancel` to back
out without doing anything.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `OnConfirm` | `EventCallback` | required |
| `OnCancel` | `EventCallback` | Cancel button only shows if this has a delegate |
| `Message` | `string` | defaults to `"Are you sure you want to log out?"` |
| `ConfirmText` | `string` | defaults to `"Log out"` |
| `CancelText` | `string` | defaults to `"Cancel"` |
| `Busy` | `bool` | disables Confirm, swaps its text to "Logging out…" |
| `ButtonAlign` | `FaAlign` | `Start` \| `Center` \| `End` (default) \| `Between` \| `Around` \| `Evenly` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
