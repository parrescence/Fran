using Microsoft.AspNetCore.Components;

namespace FactoryAspects.Components;

/// <summary>
/// One column definition for <see cref="FaDataTable{TItem}"/> — header text, how to
/// render a cell, and optionally how to sort/filter by it. A plain class with
/// `required` members rather than a constructor-per-combination, since most of these
/// are independently optional (sortable without a filter, filterable without being
/// sortable, plain display-only, ...).
/// </summary>
public sealed class FaDataTableColumn<TItem>
{
    /// <summary>Column header text.</summary>
    public required string Header { get; init; }
    /// <summary>Renders one row's cell content for this column.</summary>
    public required Func<TItem, RenderFragment> CellContent { get; init; }
    public string? CssClass { get; init; }

    /// <summary>Set to allow clicking the header to sort by this column.</summary>
    public bool Sortable { get; init; }
    /// <summary>Required when <see cref="Sortable"/> is true — the value sorting compares.</summary>
    public Func<TItem, IComparable>? SortKey { get; init; }

    /// <summary>
    /// (Key, Label) pairs for this column's filter dropdown, e.g.
    /// <c>[("open", "Open"), ("closed", "Closed")]</c> — "All" is added automatically.
    /// Leave null for a column with no filter.
    /// </summary>
    public IReadOnlyList<(string Key, string Label)>? FilterOptions { get; init; }
    /// <summary>Required when <see cref="FilterOptions"/> is set — does this row match the selected filter key?</summary>
    public Func<TItem, string, bool>? FilterPredicate { get; init; }
}
