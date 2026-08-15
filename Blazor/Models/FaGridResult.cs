namespace FaFa.Components;

/// <summary>
/// What an <see cref="FaGrid{TItem}.ItemsProvider"/> hands back for a given <see
/// cref="FaGridRequest"/> — just the requested page's rows, plus the total row count
/// across every page (needed to render "1–10 of 240" and compute the page count).
/// </summary>
public sealed class FaGridResult<TItem>
{
    /// <summary>Only this page's rows — not the whole dataset.</summary>
    public required IReadOnlyList<TItem> Items { get; init; }
    /// <summary>Total rows across all pages, after filtering.</summary>
    public required int TotalCount { get; init; }
}
