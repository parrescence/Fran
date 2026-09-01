[← Back to index](index.md)

# FaBreadcrumb

Path trail — Home › Section › current page. The last item always renders as plain
text with `aria-current="page"` even if you give it an `Href` (linking to the page
you're already on isn't real navigation); any earlier item with a null/empty `Href`
renders as plain text too instead of a dead link.

## Usage

```razor
<FaBreadcrumb Items="@(new (string, string?)[]
{
    ("Home", "/"),
    ("Orders", "/orders"),
    ("Order #4821", null),
})" />
```

## Getting the value

No bound value — plain display, driven entirely by `Items`.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Items` | `IReadOnlyList<(string Text, string? Href)>` | **required** |
| `CssClass` | `string?` | |

[← Back to index](index.md)
