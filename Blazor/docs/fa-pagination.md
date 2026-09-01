[← Back to index](index.md)

# FaPagination

Standalone windowed page-number strip — prev/next icon buttons plus first, last,
current ± 1, and a single "…" for whatever's skipped in between. `FaGrid` pages its
own data internally and doesn't use this component; reach for `FaPagination` when
you're paging something else — a list of cards, search results, anything not
already going through `FaGrid`.

## Usage

```razor
<FaPagination CurrentPage="_page" PageCount="_pageCount" OnPageChange="HandlePageChange" />

@code {
    private int _page = 1;
    private int _pageCount = 12;

    private void HandlePageChange(int page) => _page = page;
}
```

## Getting the value

Not two-way bindable — `CurrentPage` is display-only and `OnPageChange` fires with
the page the user picked; update your own state in the handler (as above) the same
way you'd drive a provider-backed `FaGrid`'s paging.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `CurrentPage` | `int` | 1-based |
| `PageCount` | `int` | **required** |
| `OnPageChange` | `EventCallback<int>` | fires with the newly picked page |
| `CssClass` | `string?` | |

[← Back to index](index.md)
