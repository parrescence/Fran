using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// Matches the alert-danger boxes already used for form errors across the app.
/// </summary>
public sealed class FaAlert : ComponentBase
{
    [Parameter] public FaAlertVariant Variant { get; set; } = FaAlertVariant.Info;

    /// <summary>XSmall/Small/Medium(default)/Large/XLarge — scales padding and text. See <see cref="FaSize"/>.</summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    [Parameter] public string? CssClass { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string VariantClass => Variant switch
    {
        FaAlertVariant.Danger => "fa-alert-danger",
        FaAlertVariant.Success => "fa-alert-success",
        _ => "fa-alert-info"
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-alert", VariantClass, FaSizeClassNames.Class("fa-alert", Size), CssClass));
        builder.AddAttribute(2, "role", "alert");
        builder.AddContent(3, ChildContent);
        builder.CloseElement();
    }
}
