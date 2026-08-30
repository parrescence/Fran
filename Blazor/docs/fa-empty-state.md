[← Back to index](index.md)

# FaEmptyState

"Nothing here yet" placeholder for an empty table/grid/list — an icon, a title, an
optional description, and optional action content (typically a `FaButton`) through
`ChildContent`. Not tied to `FaGrid`/`FaTable` specifically; drop it in wherever a
collection came back empty.

## Usage

```razor
@if (_orders.Count == 0)
{
    <FaEmptyState IconName="FaIconName.Receipt"
                  Title="No orders yet"
                  Description="Orders you place will show up here.">
        <FaButton Variant="FaButtonVariant.Primary" OnClick="GoToCatalog">Browse products</FaButton>
    </FaEmptyState>
}
```

## Getting the value

No bound value — plain display.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `IconName` | `FaIconName` | defaults to `Search` |
| `Title` | `string` | **required** |
| `Description` | `string?` | |
| `ChildContent` | `RenderFragment?` | action row, typically a `FaButton` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
