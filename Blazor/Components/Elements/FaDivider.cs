using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// A section separator — a plain rule, or a rule split around a short label (e.g.
/// "OR" between two sign-in options). Vertical divides side-by-side content instead
/// of stacked content; Label is ignored when Vertical is true (a vertical rule has
/// no natural place to put text). Renders a plain &lt;div role="separator"&gt;
/// rather than &lt;hr&gt; even for the unlabeled case, since &lt;hr&gt; can't
/// contain the label markup the other case needs and this keeps both paths the
/// same element.
/// </summary>
public sealed class FaDivider : ComponentBase
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Vertical { get; set; }
    [Parameter] public string? CssClass { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var hasLabel = !Vertical && !string.IsNullOrEmpty(Label);

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine(
            "fa-divider",
            Vertical ? "fa-divider-vertical" : null,
            hasLabel ? "fa-divider-labeled" : null,
            CssClass));
        builder.AddAttribute(2, "role", "separator");
        builder.AddAttribute(3, "aria-orientation", Vertical ? "vertical" : "horizontal");

        if (hasLabel)
        {
            builder.AddContent(4, Label);
        }

        builder.CloseElement();
    }
}
