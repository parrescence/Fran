[← Back to index](index.md)

# FaLoginForm

A ready-made username/password login form. It owns its own credentials state
internally and hands them to `OnSubmit` once submitted — FaLoginForm never calls an
auth endpoint itself, that's your `OnSubmit` handler's job.

## Usage

```razor
<FaLoginForm OnSubmit="LoginAsync" Busy="_busy" ErrorText="_errorText" />

@code {
    private bool _busy;
    private string? _errorText;

    private async Task LoginAsync(FaLoginRequest request)
    {
        _busy = true;
        _errorText = null;
        var result = await _authService.LoginAsync(request.Username, request.Password, request.RememberMe);
        _busy = false;
        if (!result.Success)
        {
            _errorText = "Incorrect username or password.";
        }
        // else: navigate away, set auth state, etc.
    }
}
```

## Getting the value

`OnSubmit` hands back an `FaLoginRequest` — `Username`, `Password`, `RememberMe`.
There's nothing to bind yourself; the form's internal fields feed straight into it.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `OnSubmit` | `EventCallback<FaLoginRequest>` | required |
| `ShowRememberMe` | `bool` | defaults to `true` |
| `Busy` | `bool` | disables Submit, swaps its text to "Logging in…" |
| `ErrorText` | `string?` | shows an `FaAlert` above the fields when set |
| `UsernameLabel` | `string` | defaults to `"Username"` — set to `"Email"` etc. as needed |
| `PasswordLabel` | `string` | defaults to `"Password"` |
| `SubmitText` | `string` | defaults to `"Log in"` |
| `FooterContent` | `RenderFragment?` | rendered between the fields and Submit — a "Forgot password?" link, etc. |
| `CssClass` | `string?` | |

[← Back to index](index.md)
