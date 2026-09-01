[← Back to index](index.md)

# FaToastHost + FaToastService

Stacked, auto-dismissing notifications. Register `FaToastService` once in your
`Program.cs`, mount `<FaToastHost />` once (typically in your root layout), then
`[Inject]` the service anywhere you want to fire a toast — a button click handler, a
service, an exception handler.

```csharp
// Program.cs
builder.Services.AddScoped<FaToastService>();
```

`Scoped` is the standard lifetime for anything tied to one user's session/circuit.
On Blazor Server this keeps one user's toasts from reaching anyone else's browser;
on Blazor WebAssembly it behaves like a singleton (one user per process there
already) with nothing extra to configure either way.

## Usage

```razor
@* In your root layout, once: *@
<FaToastHost Position="FaToastPosition.TopRight" />
```

```razor
@* Anywhere else in the app: *@
<FaButton OnClick="Save">Save</FaButton>

@code {
    [Inject] private FaToastService ToastService { get; set; } = default!;

    private async Task Save()
    {
        await SaveOrderAsync();
        ToastService.Show("Order saved", FaAlertVariant.Success);
    }
}
```

## Getting the value

No bound value — `ToastService.Show(text, variant, durationMs)` fires a toast;
there's nothing to read back.

## Parameters (FaToastHost)

| Parameter | Type | Notes |
|---|---|---|
| `Position` | `FaToastPosition` | which corner/edge to stack against; defaults to `TopRight` |

## FaToastService.Show

| Argument | Type | Notes |
|---|---|---|
| `text` | `string` | **required** |
| `variant` | `FaAlertVariant` | `Info` (default) \| `Success` \| `Danger` |
| `durationMs` | `int` | how long before auto-dismiss; defaults to `4000` |

[← Back to index](index.md)
