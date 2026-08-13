[← Back to index](index.md)

# FaCarousel&lt;TItem&gt;

A slideshow over a set of items — one slide visible at a time, with prev/next
arrows and dot navigation. Same "exactly one of `Items`/`ItemsProvider`" data-source
split as [FaGrid](fa-grid.md), just without paging: a carousel holds every item, one
at a time, not a page of them.

## Usage — in-memory list

```razor
<FaCarousel TItem="Product" Items="_products">
    <ItemTemplate Context="product">
        <img src="@product.PhotoUrl" alt="@product.Name" />
        <h4>@product.Name</h4>
    </ItemTemplate>
</FaCarousel>

@code {
    // Built directly in code — a List<T>, an array, or someDictionary.Values.ToList().
    private List<Product> _products = new() { /* ... */ };
}
```

## Usage — async-fetched (a database/API call)

```razor
<FaCarousel TItem="Product" ItemsProvider="LoadProductsAsync">
    <ItemTemplate Context="product">
        <img src="@product.PhotoUrl" alt="@product.Name" />
    </ItemTemplate>
</FaCarousel>

@code {
    private Task<IReadOnlyList<Product>> LoadProductsAsync() =>
        _productService.GetFeaturedAsync();
}
```

Fetched once, when the carousel first renders — there's no paging to re-fetch on,
just a single fetch of the whole slide set.

## Usage — hand-authored slides, not backed by a data object

```razor
<FaCarousel TItem="RenderFragment" Items="_slides" ItemTemplate="frag => frag" />

@code {
    private List<RenderFragment> _slides = new()
    {
        @<div><h4>Welcome</h4><p>First slide, plain markup.</p></div>,
        @<div><h4>Step 2</h4><p>Second slide, plain markup.</p></div>,
    };
}
```

## Getting the value

No bound value — it's a display component. Track the active slide yourself if you
need it (there's nothing to read back from FaCarousel).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Items` | `IReadOnlyList<TItem>?` | exactly one of Items/ItemsProvider required |
| `ItemsProvider` | `Func<Task<IReadOnlyList<TItem>>>?` | exactly one of Items/ItemsProvider required |
| `ItemTemplate` | `RenderFragment<TItem>` | required |
| `ShowArrows` | `bool` | defaults to `true` |
| `ShowDots` | `bool` | defaults to `true` |
| `AutoPlay` | `bool` | defaults to `false` |
| `AutoPlayIntervalMs` | `int` | defaults to `4000` |
| `CssClass` | `string?` | |

[← Back to index](index.md)
