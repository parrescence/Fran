using System.Globalization;
using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// A filled track reporting how far along something is — a budget spent, an
/// upload's percent complete, a wizard step. <see cref="Direction"/> picks which
/// edge the fill grows from; Right/Left are horizontal, Up/Down are vertical.
/// </summary>
public sealed class FaProgress : ComponentBase
{
    [Parameter] public double Value { get; set; }
    [Parameter] public double Max { get; set; } = 100;
    [Parameter] public FaProgressDirection Direction { get; set; } = FaProgressDirection.Right;
    [Parameter] public FaLoaderVariant Variant { get; set; } = FaLoaderVariant.Accent;
    [Parameter] public string? CssClass { get; set; }

    private double Percent => Max <= 0 ? 0 : Math.Clamp(Value / Max * 100, 0, 100);

    private bool IsVertical => Direction is FaProgressDirection.Up or FaProgressDirection.Down;

    private string DirectionClass => Direction switch
    {
        FaProgressDirection.Left => "fa-progress-left",
        FaProgressDirection.Up => "fa-progress-up",
        FaProgressDirection.Down => "fa-progress-down",
        _ => "fa-progress-right"
    };

    private string? VariantClass => Variant switch
    {
        FaLoaderVariant.Primary => "fa-loader-primary",
        FaLoaderVariant.Gold => "fa-loader-gold",
        FaLoaderVariant.Danger => "fa-loader-danger",
        _ => null
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var percentText = Percent.ToString(CultureInfo.InvariantCulture);

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-progress", DirectionClass, VariantClass, CssClass));
        builder.AddAttribute(2, "role", "progressbar");
        builder.AddAttribute(3, "aria-valuenow", Value);
        builder.AddAttribute(4, "aria-valuemin", 0);
        builder.AddAttribute(5, "aria-valuemax", Max);

        builder.OpenElement(6, "div");
        builder.AddAttribute(7, "class", "fa-progress-bar");
        builder.AddAttribute(8, "style", IsVertical ? $"height:{percentText}%" : $"width:{percentText}%");
        builder.CloseElement();

        builder.CloseElement();
    }
}
