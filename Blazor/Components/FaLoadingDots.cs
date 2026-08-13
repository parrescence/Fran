using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// "Loading" with a trailing ellipsis that grows one dot at a time (<c>.</c> →
/// <c>..</c> → <c>...</c>) then snaps back and repeats — a plain-text loading
/// indicator for places a spinner would feel too heavy, e.g. inline in a sentence.
/// The growing dots are pure CSS (a clipped width animated in steps), not a timer.
/// </summary>
public sealed class FaLoadingDots : ComponentBase
{
    [Parameter] public string Text { get; set; } = "Loading";
    [Parameter] public string? CssClass { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-loading-dots", CssClass));
        builder.AddAttribute(2, "role", "status");
        builder.AddAttribute(3, "aria-label", Text);

        builder.OpenElement(4, "span");
        builder.AddAttribute(5, "aria-hidden", "true");
        builder.AddContent(6, Text);
        builder.CloseElement();

        builder.OpenElement(7, "span");
        builder.AddAttribute(8, "class", "fa-loading-dots-ellipsis");
        builder.AddAttribute(9, "aria-hidden", "true");
        builder.AddContent(10, "...");
        builder.CloseElement();

        builder.CloseElement();
    }
}
