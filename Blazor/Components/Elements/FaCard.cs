using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Fran.Components;

/// <summary>
/// Bordered tile — matches the dashboard's category tiles, but with the shared orange
/// backlight border and a darker glow on focus/click instead of inline styles.
/// </summary>
public sealed class FaCard : ComponentBase
{
    [Parameter] public bool Clickable { get; set; }
    [Parameter] public bool Active { get; set; }

    /// <summary>XSmall/Small/Medium(default)/Large/XLarge — scales padding. See <see cref="FaSize"/>.</summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    /// <summary>Stretches to 100% width below the 720px breakpoint (<c>.fa-responsive</c> in <c>_responsive.scss</c>). Off by default.</summary>
    [Parameter] public bool Responsive { get; set; }

    [Parameter] public string? CssClass { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-card", FaSizeClassNames.Class("fa-card", Size), Active ? "fa-card-active" : null, Responsive ? "fa-responsive" : null, CssClass));
        builder.AddAttribute(2, "tabindex", Clickable ? "0" : null);
        builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, OnClick));
        builder.AddMultipleAttributes(4, AdditionalAttributes);
        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }
}
