[← Back to index](index.md)

# FaButton

Rounded, palette-driven button. Set `Href` to render an `<a>` styled as a button
instead of a `<button>`.

## Usage

```razor
<FaButton Variant="FaButtonVariant.Primary" OnClick="HandleSaveAsync">
    Save
</FaButton>

<FaButton Variant="FaButtonVariant.OutlineDanger" Size="FaSize.Small" OnClick="HandleDeleteAsync">
    Delete
</FaButton>

<FaButton Href="/reports" Variant="FaButtonVariant.Secondary">
    View reports
</FaButton>

@code {
    private Task HandleSaveAsync(MouseEventArgs e)
    {
        // save...
        return Task.CompletedTask;
    }

    private Task HandleDeleteAsync(MouseEventArgs e)
    {
        // delete...
        return Task.CompletedTask;
    }
}
```

## Getting the value

There's no bound value — `FaButton` is a trigger, not a field. `OnClick` receives the
raw `MouseEventArgs`, same as a native `<button onclick>`.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Variant` | `FaButtonVariant` | `Primary` \| `Secondary` \| `Outline` \| `Danger` \| `OutlineDanger` \| `Accent` \| `OutlineAccent` |
| `Size` | `FaSize` | `XSmall` \| `Small` \| `Medium` (default) \| `Large` \| `XLarge` — see [Sizing](sizing.md). Replaces the old `bool Small` parameter |
| `Responsive` | `bool` | stretches to 100% width below 720px, default `false` — see [Sizing](sizing.md#responsive) |
| `Disabled` | `bool` | |
| `Type` | `string` | HTML `type`, default `"button"` — set `"submit"` inside an `EditForm` |
| `Href` / `Target` | `string?` | set `Href` to render as `<a>` instead of `<button>` |
| `OnClick` | `EventCallback<MouseEventArgs>` | |
| `CssClass` | `string?` | extra classes |

[← Back to index](index.md)
