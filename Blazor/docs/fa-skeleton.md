[← Back to index](index.md)

# FaSkeleton

Content-shaped loading placeholder — a shimmering bar/circle/block standing in for
text, an avatar, or an image while real data is still loading. Distinct from
`FaSpinner`/`FaLoadingDots`/etc.: those signal "something is happening" in the
abstract, this signals "here specifically is what's coming," so a page's overall
loading feel can mix both (e.g. a skeleton card with a spinner in its corner).

## Usage

```razor
@if (_loading)
{
    <FaSkeleton Variant="FaSkeletonVariant.Circle" />
    <FaSkeleton Variant="FaSkeletonVariant.Text" Lines="3" />
}
else
{
    <FaAvatar ImageUrl="@_user.PhotoUrl" />
    <p>@_user.Bio</p>
}
```

### Sizing a rect skeleton (e.g. an image placeholder)

```razor
<FaSkeleton Variant="FaSkeletonVariant.Rect" Width="100%" Height="12rem" />
```

## Getting the value

No bound value — plain display.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Variant` | `FaSkeletonVariant` | `Text` (default) \| `Circle` \| `Rect` |
| `Width` / `Height` | `string?` | any CSS length, e.g. `"12rem"`, `"100%"` |
| `Lines` | `int` | `Text` variant only — stacks this many bars, the last one shorter |
| `CssClass` | `string?` | |

[← Back to index](index.md)
