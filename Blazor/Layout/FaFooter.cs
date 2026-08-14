using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Layout;

/// <summary>
/// Lighter footer band, closes out both page templates.
/// </summary>
public sealed class FaFooter : ComponentBase
{
    [Parameter, EditorRequired] public string BrandText { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "footer");
        builder.AddAttribute(1, "class", "fa-footer");

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
