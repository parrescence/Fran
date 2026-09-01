[← Back to index](index.md)

# FaChip

A small interactive tag — a selected filter, an added recipient, a removable
attachment. Distinct from `FaBadge`: a badge is a passive status label (never
clicked, never removed); a chip is something the user is actively managing.

## Usage

### A filter chip (selectable)

```razor
@foreach (var tag in _tags)
{
    <FaChip Selected="@(_selected == tag)" OnClick="@(() => _selected = tag)">
        @tag
    </FaChip>
}
```

### A removable chip

```razor
@foreach (var recipient in _recipients)
{
    <FaChip Removable="true" OnRemove="@(() => _recipients.Remove(recipient))">
        @recipient
    </FaChip>
}
```

## Getting the value

No bound value — `OnClick`/`OnRemove` fire with the click/remove; track selection
or membership in your own list the same way the examples above do.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `ChildContent` | `RenderFragment?` | the chip's label |
| `IconName` | `FaIconName?` | optional leading icon |
| `Variant` | `FaBadgeVariant` | `Neutral` (default) \| `Primary` \| `Success` \| `Danger` |
| `Selected` | `bool` | |
| `Removable` | `bool` | shows a remove (×) button |
| `Disabled` | `bool` | |
| `OnClick` | `EventCallback` | set this to make the chip clickable at all |
| `OnRemove` | `EventCallback` | fires when the remove button is clicked |
| `CssClass` | `string?` | |

[← Back to index](index.md)
