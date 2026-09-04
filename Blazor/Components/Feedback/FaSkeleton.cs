using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// Content-shaped loading placeholder — a shimmering bar/circle/block standing in
/// for text, an avatar, or an image while real data is still loading. Distinct from
/// FaSpinner/FaLoadingDots/etc.: those signal "something is happening" in the
/// abstract, this signals "here specifically is what's coming," so a page's overall
/// loading feel can mix both (e.g. a skeleton card with a spinner in its corner).
/// </summary>
public sealed class FaSkeleton : ComponentBase
{
    [Parameter] public FaSkeletonVariant Variant { get; set; } = FaSkeletonVariant.Text;
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? Height { get; set; }
    /// <summary>Only meaningful when Variant="Text" — renders this many stacked bars, the last one narrower so it reads as a paragraph's ragged last line.</summary>
    [Parameter] public int Lines { get; set; } = 1;
    [Parameter] public string? CssClass { get; set; }

    private string VariantClass => Variant switch
    {
        FaSkeletonVariant.Circle => "fa-skeleton-circle",
        FaSkeletonVariant.Rect => "fa-skeleton-rect",
        _ => "fa-skeleton-text"
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Variant != FaSkeletonVariant.Text || Lines <= 1)
        {
            RenderBar(builder, 0, isLastOfMany: false, cssClass: CssClass);
            return;
        }

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-skeleton-lines", CssClass));

        for (var i = 0; i < Lines; i++)
        {
            RenderBar(builder, 2 + i * 10, isLastOfMany: i == Lines - 1, cssClass: null);
        }

        builder.CloseElement();
    }

    private void RenderBar(RenderTreeBuilder builder, int sequence, bool isLastOfMany, string? cssClass)
    {
        builder.OpenElement(sequence, "span");
        builder.SetKey(sequence);
        builder.AddAttribute(sequence + 1, "class", CssClassNames.Combine(
            "fa-skeleton", VariantClass, isLastOfMany ? "fa-skeleton-text-last" : null, cssClass));
        builder.AddAttribute(sequence + 2, "aria-hidden", "true");

        var style = "";
        if (!string.IsNullOrEmpty(Width)) style += $"width:{Width};";
        if (!string.IsNullOrEmpty(Height)) style += $"height:{Height};";
        if (style.Length > 0)
        {
            builder.AddAttribute(sequence + 3, "style", style);
        }

        builder.CloseElement();
    }
}
