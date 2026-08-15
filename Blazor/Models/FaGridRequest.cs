namespace FaFa.Components;

/// <summary>
/// What <see cref="FaGrid{TItem}.ItemsProvider"/> is asked for on every page change,
/// rows-per-page change, header-sort click, or filter-dropdown change — everything a
/// real paged query (a database call, a paged API endpoint, ...) needs to fetch
/// exactly the rows for the requested page rather than the whole dataset. The
/// provider owns fetching (and caching/storing, if it wants to avoid re-querying a
/// page it already has) — FaGrid itself only ever holds the current page's rows in
/// memory.
/// </summary>
public sealed class FaGridRequest
{
    /// <summary>0-based.</summary>
    public required int PageIndex { get; init; }
    public required int PageSize { get; init; }

    /// <summary><see cref="FaGridColumn{TItem}.EffectiveKey"/> of the sorted column, or null if unsorted.</summary>
    public string? SortKey { get; init; }
    public bool SortAscending { get; init; } = true;

    /// <summary>
    /// Selected filter key per column, keyed by <see
    /// cref="FaGridColumn{TItem}.EffectiveKey"/>. Columns with no filter selected
    /// (still on "All") are absent, not present with an empty value.
    /// </summary>
    public IReadOnlyDictionary<string, string> Filters { get; init; } = new Dictionary<string, string>();
}
