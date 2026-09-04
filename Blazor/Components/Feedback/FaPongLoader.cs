using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// A miniature game of Pong as a loading indicator — a ball bounces around the
/// court while the two paddles bob up and down, out of phase with each other.
/// Static markup (one ball, two paddles); the movement is entirely CSS keyframes.
/// </summary>
public sealed class FaPongLoader : ComponentBase
{
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
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-pong-loader", VariantClass, CssClass));
        builder.AddAttribute(2, "role", "status");
        builder.AddAttribute(3, "aria-label", "Loading");

        builder.OpenElement(4, "span");
        builder.AddAttribute(5, "class", "fa-pong-paddle fa-pong-paddle-left");
        builder.CloseElement();

        builder.OpenElement(6, "span");
        builder.AddAttribute(7, "class", "fa-pong-ball");
        builder.CloseElement();

        builder.OpenElement(8, "span");
        builder.AddAttribute(9, "class", "fa-pong-paddle fa-pong-paddle-right");
        builder.CloseElement();

        builder.CloseElement();
    }
}
