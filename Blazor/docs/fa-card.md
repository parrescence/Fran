[← Back to index](index.md)

# FaCard

Bordered tile with the shared orange backlight border. Set `Clickable` to make it
focusable/keyboard-activatable, and `Active` to highlight the currently-selected one.

## Usage

```razor
@foreach (var item in _items)
{
    <FaCard Clickable="true"
            Active="@(_selectedId == item.Id)"
            OnClick="() => Select(item.Id)">
        <h3>@item.Title</h3>
        <p>@item.Summary</p>
    </FaCard>
}

@code {
    private int? _selectedId;
    private void Select(int id) => _selectedId = id;
}
```

## Getting the value

`FaCard` has no bound value of its own — it's a container. Track "which card is
selected" yourself (as above) and pass it back in via `Active`.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Clickable` | `bool` | adds `tabindex="0"` so it's keyboard-focusable |
| `Active` | `bool` | highlighted border/glow |
| `OnClick` | `EventCallback<MouseEventArgs>` | |
| `CssClass` | `string?` | |

[← Back to index](index.md)
