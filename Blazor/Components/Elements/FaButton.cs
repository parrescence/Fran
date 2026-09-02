using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

/// <summary>
/// Rounded, palette-driven button. Variant maps to one of the fa-btn-* classes in
/// theme.css; focus state (a darker/glowing border) is handled entirely in CSS via
/// :focus-visible.
/// </summary>
/// <remarks>
/// Set <see cref="Href"/> to render an anchor styled as a button instead of a
/// <c>&lt;button&gt;</c> element — this absorbs what used to be a separate
/// "link styled as a button" component; there's no behavioral difference beyond the
/// element and its href/target attributes.
/// </remarks>
public sealed class FaButton : ComponentBase
{
    [Parameter] public FaButtonVariant Variant { get; set; } = FaButtonVariant.Primary;

    /// <summary>
    /// XSmall/Small/Medium(default)/Large/XLarge — see <see cref="FaSize"/>. Replaces
    /// the old <c>bool Small</c> parameter (breaking change — <c>Small="true"</c>
    /// becomes <c>Size="FaSize.Small"</c>).
    /// </summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    /// <summary>
    /// Stretches to 100% width below the 720px breakpoint (<c>.fa-responsive</c> in
    /// <c>_responsive.scss</c>) — off by default, since a button's intrinsic width is
    /// usually exactly what's wanted even on small screens. Opt in for a button that
    /// should read as the primary mobile action (e.g. a form's submit button).
    /// </summary>
    [Parameter] public bool Responsive { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? Target { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string VariantClass => Variant switch
    {
        FaButtonVariant.Primary => "fa-btn-primary",
        FaButtonVariant.Secondary => "fa-btn-secondary",
        FaButtonVariant.Outline => "fa-btn-outline",
        FaButtonVariant.Danger => "fa-btn-danger",
        FaButtonVariant.OutlineDanger => "fa-btn-outline-danger",
        FaButtonVariant.Accent => "fa-btn-accent",
        FaButtonVariant.OutlineAccent => "fa-btn-outline-accent",
        _ => "fa-btn-primary"
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var isLink = !string.IsNullOrEmpty(Href);

        builder.OpenElement(0, isLink ? "a" : "button");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-btn", VariantClass, FaSizeClassNames.Class("fa-btn", Size), Responsive ? "fa-responsive" : null, CssClass));

        if (isLink)
        {
            builder.AddAttribute(2, "href", Href);
            builder.AddAttribute(3, "target", Target);
            // Anchors have no native disabled state; aria-disabled communicates it to
            // assistive tech and CSS can style [aria-disabled] to look/act inert.
            builder.AddAttribute(4, "aria-disabled", Disabled ? "true" : null);
        }
        else
        {
            builder.AddAttribute(5, "type", Type);
            builder.AddAttribute(6, "disabled", Disabled);
        }

        builder.AddAttribute(7, "onclick", EventCallback.Factory.Create(this, OnClick));
        builder.AddMultipleAttributes(8, AdditionalAttributes);
        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}
