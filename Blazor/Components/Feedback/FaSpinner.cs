using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// The classic rotating-ring spinner — a button's busy state, a panel waiting on
/// data, anywhere a compact "working on it" indicator is enough on its own.
/// </summary>
public sealed class FaSpinner : ComponentBase
{
    [Parameter] public string Size { get; set; } = "24px";
    [Parameter] public FaLoaderVariant Variant { get; set; } = FaLoaderVariant.Accent;
    [Parameter] public string Label { get; set; } = "Loading";
    [Parameter] public string? CssClass { get; set; }

    private string? VariantClass => Variant switch
    {
        FaLoaderVariant.Primary => "fa-loader-primary",
        FaLoaderVariant.Gold => "fa-loader-gold",
        FaLoaderVariant.Danger => "fa-loader-danger",
        _ => null
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-spinner", VariantClass, CssClass));
        builder.AddAttribute(2, "style", $"width:{Size};height:{Size}");
        builder.AddAttribute(3, "role", "status");
        builder.AddAttribute(4, "aria-label", Label);
        builder.CloseElement();
    }
}
