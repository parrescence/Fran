using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Layout;

/// <summary>
/// Lighter footer band, closes out both page templates.
/// </summary>
public sealed class FaFooter : ComponentBase
{
    [Parameter, EditorRequired] public string BrandText { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Whether the footer scrolls away with the page (default) or stays pinned to the bottom.</summary>
    [Parameter] public FaNavPosition Position { get; set; } = FaNavPosition.Standard;

    private string? PositionClass => Position switch
    {
        FaNavPosition.Sticky => "fa-footer-sticky",
        FaNavPosition.Floating => "fa-footer-floating",
        _ => null
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "footer");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-footer", PositionClass));

        if (ChildContent is not null)
        {
            builder.AddContent(2, ChildContent);
        }
        else
        {
            builder.OpenElement(3, "span");
            builder.AddContent(4, $"© {DateTime.UtcNow.Year} {BrandText}");
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
