using Fran.Icons;
using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// Standalone page-number strip — prev/next icon buttons plus a windowed run of page
/// numbers (first, last, current ± 1, a single "…" for whatever's skipped
/// in between). FaGrid pages its own data internally and doesn't use this
/// component; reach for FaPagination when you're paging something else — a list of
/// cards, search results, anything not already going through FaGrid.
/// </summary>
public sealed class FaPagination : ComponentBase
{
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter, EditorRequired] public int PageCount { get; set; }
    [Parameter] public EventCallback<int> OnPageChange { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private Task GoAsync(int page)
    {
        if (page < 1 || page > PageCount || page == CurrentPage)
            return Task.CompletedTask;

        return OnPageChange.InvokeAsync(page);
    }

    // First, last, and current-1..current+1 always show; a gap of exactly one
    // skipped page shows that page instead of an ellipsis (no "...4..." standing in
    // for a single page 4), anything wider than that collapses to a single "...".
    private IEnumerable<int?> VisiblePages()
    {
        var pages = new SortedSet<int> { 1, PageCount };
        for (var p = CurrentPage - 1; p <= CurrentPage + 1; p++)
        {
            if (p >= 1 && p <= PageCount)
                pages.Add(p);
        }

        int? previous = null;
        foreach (var page in pages)
        {
            if (previous is int prev && page - prev > 1)
            {
                yield return page - prev == 2 ? prev + 1 : null;
            }
            yield return page;
            previous = page;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-pagination", CssClass));
        builder.AddAttribute(2, "aria-label", "Pagination");

        var seq = 3;

        RenderNavButton(builder, ref seq, FaIconName.ChevronLeft, "Previous page", CurrentPage - 1, CurrentPage <= 1);

        foreach (var page in VisiblePages())
        {
            if (page is null)
            {
                builder.OpenElement(seq++, "span");
                builder.AddAttribute(seq++, "class", "fa-pagination-ellipsis");
                builder.AddAttribute(seq++, "aria-hidden", "true");
                builder.AddContent(seq++, "…");
                builder.CloseElement();
                continue;
            }

            var pageNumber = page.Value;
            var isActive = pageNumber == CurrentPage;

            builder.OpenElement(seq++, "button");
            builder.SetKey(pageNumber);
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-pagination-page", isActive ? "fa-pagination-page-active" : null));
            builder.AddAttribute(seq++, "aria-current", isActive ? "page" : null);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => GoAsync(pageNumber)));
            builder.AddContent(seq++, pageNumber);
            builder.CloseElement();
        }

        RenderNavButton(builder, ref seq, FaIconName.ChevronRight, "Next page", CurrentPage + 1, CurrentPage >= PageCount);

        builder.CloseElement();
    }

    private void RenderNavButton(RenderTreeBuilder builder, ref int seq, FaIconName icon, string label, int targetPage, bool disabled)
    {
        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-pagination-nav");
        builder.AddAttribute(seq++, "aria-label", label);
        builder.AddAttribute(seq++, "disabled", disabled);
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => GoAsync(targetPage)));
        builder.OpenComponent<FaIcon>(seq++);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Name), icon);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Color), FaIconColor.Black);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 14);
        builder.CloseComponent();
        builder.CloseElement();
    }
}
