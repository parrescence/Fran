using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// Small pill label — invite status, budget over/under, etc.
/// </summary>
public sealed class FaBadge : ComponentBase
{
    [Parameter] public FaBadgeVariant Variant { get; set; } = FaBadgeVariant.Neutral;
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string? VariantClass => Variant switch
    {
        FaBadgeVariant.Success => "fa-badge-success",
        FaBadgeVariant.Danger => "fa-badge-danger",
        FaBadgeVariant.Primary => "fa-badge-primary",
        _ => null
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-badge", VariantClass, CssClass));
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}
