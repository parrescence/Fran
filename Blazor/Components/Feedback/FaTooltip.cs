using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Components;

/// <summary>
/// Hover/focus label for whatever's wrapped in ChildContent — pure CSS (:hover,
/// :focus-within on the outer wrapper), no JS positioning logic, so it never needs
/// to measure anything or react to scroll/resize. Position is a hint, not a smart
/// auto-flip: pick the side that actually has room in your layout. Keyboard
/// accessibility rides on :focus-within rather than a forced tabindex on the
/// wrapper, so it shows correctly when ChildContent is itself focusable (a button,
/// a link) without adding a second, redundant tab stop — wrapping plain
/// non-focusable content (bare text, an icon) means the tooltip is mouse/hover-only,
/// same as native `title` would be.
/// </summary>
public sealed class FaTooltip : ComponentBase
{
    [Parameter, EditorRequired] public string Text { get; set; } = "";
    [Parameter] public FaTooltipPosition Position { get; set; } = FaTooltipPosition.Top;
    [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;
    [Parameter] public string? CssClass { get; set; }

    private string PositionClass => Position switch
    {
        FaTooltipPosition.Bottom => "fa-tooltip-bottom",
        FaTooltipPosition.Left => "fa-tooltip-left",
        FaTooltipPosition.Right => "fa-tooltip-right",
        _ => "fa-tooltip-top"
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-tooltip", CssClass));

        builder.OpenElement(2, "span");
        builder.AddAttribute(3, "class", "fa-tooltip-trigger");
        builder.AddContent(4, ChildContent);
        builder.CloseElement();

        builder.OpenElement(5, "span");
        builder.AddAttribute(6, "class", CssClassNames.Combine("fa-tooltip-bubble", PositionClass));
        builder.AddAttribute(7, "role", "tooltip");
        builder.AddContent(8, Text);
        builder.CloseElement();

        builder.CloseElement();
    }
}
