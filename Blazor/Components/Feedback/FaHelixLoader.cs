using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// A DNA-helix-style loader: a row of "rungs", each a top/bottom dot pair that
/// swings in opposite phase (the twist) while a staggered per-rung animation delay
/// sends that twist traveling down the row, and the whole loader breathes in and
/// out (the expand/contract). Purely CSS keyframes — <see cref="RungCount"/> just
/// controls how many rungs get rendered.
/// </summary>
public sealed class FaHelixLoader : ComponentBase
{
    private const int MaxStaggeredRungs = 6;

    [Parameter] public int RungCount { get; set; } = 5;
    [Parameter] public FaLoaderVariant Variant { get; set; } = FaLoaderVariant.Accent;
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
        var seq = 0;

        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-helix-loader", VariantClass, CssClass));
        builder.AddAttribute(seq++, "role", "status");
        builder.AddAttribute(seq++, "aria-label", "Loading");

        var rungCount = Math.Clamp(RungCount, 1, MaxStaggeredRungs);
        for (var i = 0; i < rungCount; i++)
        {
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "fa-helix-rung");

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "fa-helix-dot fa-helix-dot-top");
            builder.CloseElement();

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "fa-helix-dot fa-helix-dot-bottom");
            builder.CloseElement();

            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
