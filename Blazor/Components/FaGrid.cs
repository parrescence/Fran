using FactoryAspects.Icons;
using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FactoryAspects.Components;

/// <summary>
/// A step up from the plain <see cref="FaTable"/>: owns paging/rows-per-page and
/// per-column sorting/filtering itself instead of making every consumer re-implement
/// that loop. Works in one of two mutually exclusive modes, set via exactly one of
/// <see cref="Items"/>/<see cref="ItemsProvider"/> (enforced in
/// <see cref="OnParametersSet"/>, same runtime-validation style as FaToggle's
/// Options check):
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b><see cref="Items"/></b> — the whole (unpaged, unfiltered) dataset in
/// memory. FaGrid does the filtering/sorting/paging itself, client-side. Simplest
/// option; fine for small-to-medium in-memory lists.</item>
/// <item><b><see cref="ItemsProvider"/></b> — FaGrid never holds more than one page
/// in memory. Every page change, rows-per-page change, header-sort click, or filter
/// change builds an <see cref="FaGridRequest"/> (page index/size, sort key, selected
/// filters) and awaits it; the provider — a real paged query, an API call, whatever —
/// returns just that page's rows plus a total count as an <see
/// cref="FaGridResult{TItem}"/>. FaGrid itself does no caching of prior pages; if a
/// consumer wants Next/Previous to skip re-fetching a page it's already seen, that
/// caching lives in the provider function's own closure, not in FaGrid.</item>
/// </list>
/// In provider mode, <see cref="FaGridColumn{TItem}.SortKey"/>/
/// <see cref="FaGridColumn{TItem}.FilterPredicate"/> are never invoked — sorting and
/// filtering are the provider's job, driven by <see cref="FaGridRequest.SortKey"/>/
/// <see cref="FaGridRequest.Filters"/>.
/// </remarks>
public sealed class FaGrid<TItem> : ComponentBase
{
    [Parameter] public IReadOnlyList<TItem>? Items { get; set; }
    [Parameter] public Func<FaGridRequest, Task<FaGridResult<TItem>>>? ItemsProvider { get; set; }

    [Parameter, EditorRequired] public IReadOnlyList<FaGridColumn<TItem>> Columns { get; set; } = Array.Empty<FaGridColumn<TItem>>();

    [Parameter] public int PageSize { get; set; } = 10;
    [Parameter] public EventCallback<int> PageSizeChanged { get; set; }
    [Parameter] public IReadOnlyList<int> PageSizeOptions { get; set; } = new[] { 10, 25, 50, 100 };

    [Parameter] public string? EmptyText { get; set; } = "No rows to show.";
    [Parameter] public string? CssClass { get; set; }

    private bool IsProviderMode => ItemsProvider is not null;

    // Keyed by column index rather than Header text — two columns can share a header
    // (e.g. two "Amount" columns), and the index is stable across renders either way.
    private readonly Dictionary<int, string> _filters = new();
    private int _sortColumnIndex = -1;
    private bool _sortAscending = true;
    private int _page;

    // Provider-mode state: only ever the current page, never the whole dataset.
    private IReadOnlyList<TItem> _providerItems = Array.Empty<TItem>();
    private int _providerTotalCount;
    private bool _isLoading;
    private int _requestVersion;

    protected override void OnParametersSet()
    {
        var hasItems = Items is not null;
        var hasProvider = ItemsProvider is not null;
        if (hasItems == hasProvider)
        {
            throw new ArgumentException(
                $"FaGrid requires exactly one of {nameof(Items)} or {nameof(ItemsProvider)} to be set.",
                hasItems ? nameof(ItemsProvider) : nameof(Items));
        }
    }

    protected override Task OnInitializedAsync() => IsProviderMode ? LoadAsync() : Task.CompletedTask;

    private FaGridRequest BuildRequest() => new()
    {
        PageIndex = _page,
        PageSize = PageSize,
        SortKey = _sortColumnIndex >= 0 && _sortColumnIndex < Columns.Count ? Columns[_sortColumnIndex].EffectiveKey : null,
        SortAscending = _sortAscending,
        Filters = _filters
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .ToDictionary(kvp => Columns[kvp.Key].EffectiveKey, kvp => kvp.Value)
    };

    private async Task LoadAsync()
    {
        if (ItemsProvider is null)
        {
            return;
        }

        _isLoading = true;
        var version = ++_requestVersion;
        var result = await ItemsProvider(BuildRequest());
        if (version != _requestVersion)
        {
            return; // superseded by a later request (e.g. rapid page clicks)
        }

        _providerItems = result.Items;
        _providerTotalCount = result.TotalCount;
        _isLoading = false;
    }

    private IReadOnlyList<TItem> FilteredSortedClientItems()
    {
        IEnumerable<TItem> query = Items ?? Array.Empty<TItem>();

        foreach (var (columnIndex, filterKey) in _filters)
        {
            if (string.IsNullOrEmpty(filterKey))
            {
                continue;
            }
            var predicate = Columns[columnIndex].FilterPredicate;
            if (predicate is not null)
            {
                query = query.Where(item => predicate(item, filterKey));
            }
        }

        if (_sortColumnIndex >= 0 && _sortColumnIndex < Columns.Count && Columns[_sortColumnIndex].SortKey is { } sortKey)
        {
            query = _sortAscending ? query.OrderBy(sortKey) : query.OrderByDescending(sortKey);
        }

        return query.ToList();
    }

    private Task SetFilterAsync(int columnIndex, string key)
    {
        _filters[columnIndex] = key;
        _page = 0;
        return IsProviderMode ? LoadAsync() : Task.CompletedTask;
    }

    private Task ToggleSortAsync(int columnIndex)
    {
        if (_sortColumnIndex == columnIndex)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumnIndex = columnIndex;
            _sortAscending = true;
        }
        return IsProviderMode ? LoadAsync() : Task.CompletedTask;
    }

    private async Task SetPageSizeAsync(int size)
    {
        PageSize = size;
        _page = 0;
        await PageSizeChanged.InvokeAsync(size);
        if (IsProviderMode)
        {
            await LoadAsync();
        }
    }

    private Task GoToPageAsync(int page)
    {
        _page = page;
        return IsProviderMode ? LoadAsync() : Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        IReadOnlyList<TItem> pageItems;
        int totalCount;

        if (IsProviderMode)
        {
            pageItems = _providerItems;
            totalCount = _providerTotalCount;
        }
        else
        {
            var filteredSorted = FilteredSortedClientItems();
            totalCount = filteredSorted.Count;
            var pageCount = ComputePageCount(totalCount);
            _page = Math.Clamp(_page, 0, pageCount - 1);
            pageItems = PageSize > 0 ? filteredSorted.Skip(_page * PageSize).Take(PageSize).ToList() : filteredSorted;
        }

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-grid", CssClass));

        builder.OpenElement(2, "table");
        builder.AddAttribute(3, "class", "fa-table");

        RenderHeader(builder, 10);

        builder.OpenElement(1000, "tbody");
        if (pageItems.Count == 0)
        {
            builder.OpenElement(1001, "tr");
            builder.OpenElement(1002, "td");
            builder.AddAttribute(1003, "colspan", Columns.Count);
            builder.AddAttribute(1004, "class", "fa-grid-empty");
            builder.AddContent(1005, _isLoading ? "Loading…" : EmptyText);
            builder.CloseElement();
            builder.CloseElement();
        }
        else
        {
            var rowSeq = 1010;
            foreach (var item in pageItems)
            {
                builder.OpenElement(rowSeq++, "tr");
                foreach (var column in Columns)
                {
                    builder.OpenElement(rowSeq++, "td");
                    if (column.CssClass is not null)
                    {
                        builder.AddAttribute(rowSeq++, "class", column.CssClass);
                    }
                    builder.AddContent(rowSeq++, column.CellContent(item));
                    builder.CloseElement();
                }
                builder.CloseElement();
            }
        }
        builder.CloseElement();

        builder.CloseElement();

        RenderPagination(builder, 100000, totalCount, ComputePageCount(totalCount));

        builder.CloseElement();
    }

    private int ComputePageCount(int totalCount) =>
        PageSize > 0 ? Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize)) : 1;

    private void RenderHeader(RenderTreeBuilder builder, int sequence)
    {
        builder.OpenElement(sequence, "thead");

        builder.OpenElement(sequence + 1, "tr");
        var seq = sequence + 2;
        for (var i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            var columnIndex = i;

            builder.OpenElement(seq++, "th");
            if (column.Sortable)
            {
                builder.OpenElement(seq++, "button");
                builder.AddAttribute(seq++, "type", "button");
                builder.AddAttribute(seq++, "class", "fa-grid-sort-btn");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => ToggleSortAsync(columnIndex)));
                builder.AddContent(seq++, column.Header);
                if (_sortColumnIndex == columnIndex)
                {
                    builder.OpenComponent<FaIcon>(seq++);
                    builder.AddComponentParameter(seq++, nameof(FaIcon.Name), _sortAscending ? FaIconName.ChevronRight : FaIconName.ChevronLeft);
                    builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 10);
                    builder.CloseComponent();
                }
                builder.CloseElement();
            }
            else
            {
                builder.AddContent(seq++, column.Header);
            }
            builder.CloseElement();
        }
        builder.CloseElement();

        // Second header row for per-column filter dropdowns — only rendered when at
        // least one column actually has FilterOptions, so plain (no-filter) grids
        // don't grow an empty row.
        if (Columns.Any(c => c.FilterOptions is { Count: > 0 }))
        {
            builder.OpenElement(seq++, "tr");
            builder.AddAttribute(seq++, "class", "fa-grid-filter-row");
            for (var i = 0; i < Columns.Count; i++)
            {
                var column = Columns[i];
                var columnIndex = i;

                builder.OpenElement(seq++, "th");
                if (column.FilterOptions is { Count: > 0 } options)
                {
                    builder.OpenElement(seq++, "select");
                    builder.AddAttribute(seq++, "class", "fa-select fa-grid-filter-select");
                    builder.AddAttribute(seq++, "value", _filters.GetValueOrDefault(columnIndex, ""));
                    builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetFilterAsync(columnIndex, e.Value?.ToString() ?? "")));

                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(seq++, "value", "");
                    builder.AddContent(seq++, "All");
                    builder.CloseElement();

                    foreach (var (key, label) in options)
                    {
                        builder.OpenElement(seq++, "option");
                        builder.SetKey(key);
                        builder.AddAttribute(seq++, "value", key);
                        builder.AddContent(seq++, label);
                        builder.CloseElement();
                    }

                    builder.CloseElement();
                }
                builder.CloseElement();
            }
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    private void RenderPagination(RenderTreeBuilder builder, int sequence, int totalCount, int pageCount)
    {
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "fa-grid-pagination");

        builder.OpenElement(sequence + 2, "div");
        builder.AddAttribute(sequence + 3, "class", "fa-grid-pagination-size");
        builder.OpenElement(sequence + 4, "label");
        builder.AddContent(sequence + 5, "Rows per page");
        builder.OpenElement(sequence + 6, "select");
        builder.AddAttribute(sequence + 7, "class", "fa-select");
        builder.AddAttribute(sequence + 8, "value", PageSize.ToString());
        builder.AddAttribute(sequence + 9, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
            int.TryParse(e.Value?.ToString(), out var size) ? SetPageSizeAsync(size) : Task.CompletedTask));
        var seq = sequence + 10;
        foreach (var option in PageSizeOptions)
        {
            builder.OpenElement(seq++, "option");
            builder.SetKey(option);
            builder.AddAttribute(seq++, "value", option.ToString());
            builder.AddContent(seq++, option.ToString());
            builder.CloseElement();
        }
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-grid-pagination-info");
        var firstRow = totalCount == 0 ? 0 : (_page * PageSize) + 1;
        var lastRow = Math.Min(totalCount, (_page + 1) * PageSize);
        builder.AddContent(seq++, $"{firstRow}–{lastRow} of {totalCount}");
        builder.CloseElement();

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-grid-pagination-nav");

        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-grid-pagination-btn");
        builder.AddAttribute(seq++, "disabled", _page <= 0 || _isLoading);
        builder.AddAttribute(seq++, "aria-label", "Previous page");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => GoToPageAsync(Math.Max(0, _page - 1))));
        builder.OpenComponent<FaIcon>(seq++);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Name), FaIconName.ChevronLeft);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 12);
        builder.CloseComponent();
        builder.CloseElement();

        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", "fa-grid-pagination-page");
        builder.AddContent(seq++, $"Page {_page + 1} of {pageCount}");
        builder.CloseElement();

        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-grid-pagination-btn");
        builder.AddAttribute(seq++, "disabled", _page >= pageCount - 1 || _isLoading);
        builder.AddAttribute(seq++, "aria-label", "Next page");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => GoToPageAsync(Math.Min(pageCount - 1, _page + 1))));
        builder.OpenComponent<FaIcon>(seq++);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Name), FaIconName.ChevronRight);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 12);
        builder.CloseComponent();
        builder.CloseElement();

        builder.CloseElement();

        builder.CloseElement();
    }
}
