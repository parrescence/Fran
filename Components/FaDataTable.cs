using FactoryAspects.Icons;
using FactoryAspects.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FactoryAspects.Components;

/// <summary>
/// A step up from the plain <see cref="FaTable"/>: takes the untouched
/// <see cref="Items"/> and <see cref="Columns"/> and owns per-column filtering,
/// column-header sorting, and paging/rows-per-page itself, rather than making every
/// consumer re-implement that loop. Filtering is opt-in per column via
/// <see cref="FaDataTableColumn{TItem}.FilterOptions"/> (a plain (Key, Label) tuple
/// list, same shape as FaToggle/FaRadioGroup's Options) plus a
/// <see cref="FaDataTableColumn{TItem}.FilterPredicate"/> the caller supplies —
/// this component has no idea what "matches" means for arbitrary
/// <typeparamref name="TItem"/> data, so it never guesses.
/// </summary>
public sealed class FaDataTable<TItem> : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();
    [Parameter, EditorRequired] public IReadOnlyList<FaDataTableColumn<TItem>> Columns { get; set; } = Array.Empty<FaDataTableColumn<TItem>>();

    [Parameter] public int PageSize { get; set; } = 10;
    [Parameter] public EventCallback<int> PageSizeChanged { get; set; }
    [Parameter] public IReadOnlyList<int> PageSizeOptions { get; set; } = new[] { 10, 25, 50, 100 };

    [Parameter] public string? EmptyText { get; set; } = "No rows to show.";
    [Parameter] public string? CssClass { get; set; }

    // Keyed by column index rather than Header text — two columns can share a header
    // (e.g. two "Amount" columns), and the index is stable across renders either way.
    private readonly Dictionary<int, string> _filters = new();
    private int _sortColumnIndex = -1;
    private bool _sortAscending = true;
    private int _page;

    private IReadOnlyList<TItem> FilteredSortedItems()
    {
        IEnumerable<TItem> query = Items;

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

    private void SetFilter(int columnIndex, string key)
    {
        _filters[columnIndex] = key;
        _page = 0;
    }

    private void ToggleSort(int columnIndex)
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
    }

    private async Task SetPageSizeAsync(int size)
    {
        PageSize = size;
        _page = 0;
        await PageSizeChanged.InvokeAsync(size);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var filteredSorted = FilteredSortedItems();
        var totalCount = filteredSorted.Count;
        var pageCount = PageSize > 0 ? Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize)) : 1;
        _page = Math.Clamp(_page, 0, pageCount - 1);
        var pageItems = PageSize > 0 ? filteredSorted.Skip(_page * PageSize).Take(PageSize).ToList() : filteredSorted;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ClassNames.Combine("fa-datatable", CssClass));

        builder.OpenElement(2, "table");
        builder.AddAttribute(3, "class", "fa-table");

        RenderHeader(builder, 10);

        builder.OpenElement(1000, "tbody");
        if (pageItems.Count == 0)
        {
            builder.OpenElement(1001, "tr");
            builder.OpenElement(1002, "td");
            builder.AddAttribute(1003, "colspan", Columns.Count);
            builder.AddAttribute(1004, "class", "fa-datatable-empty");
            builder.AddContent(1005, EmptyText);
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

        RenderPagination(builder, 100000, totalCount, pageCount);

        builder.CloseElement();
    }

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
                builder.AddAttribute(seq++, "class", "fa-datatable-sort-btn");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => ToggleSort(columnIndex)));
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
        // least one column actually has FilterOptions, so plain (no-filter) tables
        // don't grow an empty row.
        if (Columns.Any(c => c.FilterOptions is { Count: > 0 }))
        {
            builder.OpenElement(seq++, "tr");
            builder.AddAttribute(seq++, "class", "fa-datatable-filter-row");
            for (var i = 0; i < Columns.Count; i++)
            {
                var column = Columns[i];
                var columnIndex = i;

                builder.OpenElement(seq++, "th");
                if (column.FilterOptions is { Count: > 0 } options)
                {
                    builder.OpenElement(seq++, "select");
                    builder.AddAttribute(seq++, "class", "fa-select fa-datatable-filter-select");
                    builder.AddAttribute(seq++, "value", _filters.GetValueOrDefault(columnIndex, ""));
                    builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetFilter(columnIndex, e.Value?.ToString() ?? "")));

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
        builder.AddAttribute(sequence + 1, "class", "fa-datatable-pagination");

        builder.OpenElement(sequence + 2, "div");
        builder.AddAttribute(sequence + 3, "class", "fa-datatable-pagination-size");
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
        builder.AddAttribute(seq++, "class", "fa-datatable-pagination-info");
        var firstRow = totalCount == 0 ? 0 : (_page * PageSize) + 1;
        var lastRow = Math.Min(totalCount, (_page + 1) * PageSize);
        builder.AddContent(seq++, $"{firstRow}–{lastRow} of {totalCount}");
        builder.CloseElement();

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-datatable-pagination-nav");

        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-datatable-pagination-btn");
        builder.AddAttribute(seq++, "disabled", _page <= 0);
        builder.AddAttribute(seq++, "aria-label", "Previous page");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => _page = Math.Max(0, _page - 1)));
        builder.OpenComponent<FaIcon>(seq++);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Name), FaIconName.ChevronLeft);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 12);
        builder.CloseComponent();
        builder.CloseElement();

        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", "fa-datatable-pagination-page");
        builder.AddContent(seq++, $"Page {_page + 1} of {pageCount}");
        builder.CloseElement();

        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-datatable-pagination-btn");
        builder.AddAttribute(seq++, "disabled", _page >= pageCount - 1);
        builder.AddAttribute(seq++, "aria-label", "Next page");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => _page = Math.Min(pageCount - 1, _page + 1)));
        builder.OpenComponent<FaIcon>(seq++);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Name), FaIconName.ChevronRight);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 12);
        builder.CloseComponent();
        builder.CloseElement();

        builder.CloseElement();

        builder.CloseElement();
    }
}
