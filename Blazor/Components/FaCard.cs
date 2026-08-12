using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FactoryAspects.Components;

/// <summary>
/// Bordered tile — matches the dashboard's category tiles, but with the shared orange
/// backlight border and a darker glow on focus/click instead of inline styles.
/// </summary>
public sealed class FaCard : ComponentBase
{
    [Parameter] public bool Clickable { get; set; }
    [Parameter] public bool Active { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-card", Active ? "fa-card-active" : null, CssClass));
        builder.AddAttribute(2, "tabindex", Clickable ? "0" : null);
        builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, OnClick));
        builder.AddMultipleAttributes(4, AdditionalAttributes);
        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }
}
