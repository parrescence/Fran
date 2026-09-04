using Microsoft.AspNetCore.Components;

namespace Fran.Components;

/// <summary>
/// One column definition for <see cref="FaGrid{TItem}"/> — header text, how to render
/// a cell, and optionally how to sort/filter by it. A plain class with `required`
/// members rather than a constructor-per-combination, since most of these are
/// independently optional (sortable without a filter, filterable without being
/// sortable, plain display-only, ...).
/// </summary>
public sealed class FaGridColumn<TItem>
{
    /// <summary>Column header text.</summary>
    public required string Header { get; init; }
    /// <summary>Renders one row's cell content for this column.</summary>
    public required Func<TItem, RenderFragment> CellContent { get; init; }
    public string? CssClass { get; init; }

    /// <summary>
    /// Stable identifier for this column, used in <see cref="FaGridRequest"/> when the
    /// grid is driven by an <see cref="FaGrid{TItem}.ItemsProvider"/> (so the caller's
    /// query can tell which real column to sort/filter by, independent of display
    /// order or header text). Defaults to <see cref="Header"/> when not set — only
    /// give columns distinct Keys explicitly if two share a Header, or if the sort/
    /// filter identifier a provider expects differs from the header text (e.g.
    /// Header "Amount ($)" but the provider wants "amount").
    /// </summary>
    public string? Key { get; init; }

    /// <summary>Set to allow clicking the header to sort by this column.</summary>
    public bool Sortable { get; init; }
    /// <summary>
    /// Required when <see cref="Sortable"/> is true and there's no <see
    /// cref="FaGrid{TItem}.ItemsProvider"/> — the value client-side sorting compares.
    /// Ignored in provider mode, where sorting is the provider's own job (see <see
    /// cref="FaGridRequest.SortKey"/>).
    /// </summary>
    public Func<TItem, IComparable>? SortKey { get; init; }

    /// <summary>
    /// (Key, Label) pairs for this column's filter dropdown, e.g.
    /// <c>[("open", "Open"), ("closed", "Closed")]</c> — "All" is added automatically.
    /// Leave null for a column with no filter.
    /// </summary>
    public IReadOnlyList<(string Key, string Label)>? FilterOptions { get; init; }
    /// <summary>
    /// Required when <see cref="FilterOptions"/> is set and there's no <see
    /// cref="FaGrid{TItem}.ItemsProvider"/> — does this row match the selected filter
    /// key? Ignored in provider mode, where filtering is the provider's own job (see
    /// <see cref="FaGridRequest.Filters"/>).
    /// </summary>
    public Func<TItem, string, bool>? FilterPredicate { get; init; }

    internal string EffectiveKey => Key ?? Header;
}
