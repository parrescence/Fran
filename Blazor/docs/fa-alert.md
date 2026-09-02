[← Back to index](index.md)

# FaAlert

Info/success/danger banner — matches the boxes used for form errors.

## Usage

```razor
<FaAlert Variant="FaAlertVariant.Danger">
    Something went wrong saving your changes.
</FaAlert>

<FaAlert Variant="FaAlertVariant.Success">
    Saved.
</FaAlert>
```

## Getting the value

No bound value — it's a static banner. Show/hide it yourself with an `@if` around
whatever error/success state you're tracking:

```razor
@if (_errorMessage is not null)
{
    <FaAlert Variant="FaAlertVariant.Danger">@_errorMessage</FaAlert>
}
```

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Variant` | `FaAlertVariant` | `Info` (default) \| `Success` \| `Danger` |
| `Size` | `FaSize` | `XSmall` \| `Small` \| `Medium` (default) \| `Large` \| `XLarge` — see [Sizing](sizing.md) |
| `CssClass` | `string?` | |

[← Back to index](index.md)
