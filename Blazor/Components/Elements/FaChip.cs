using Fran.Icons;
using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Fran.Components;

/// <summary>
/// A small interactive tag — a selected filter, an added recipient, a removable
/// attachment. Distinct from FaBadge: a badge is a passive status label (never
/// clicked, never removed); a chip is something the user is actively managing.
/// Always renders as a &lt;span&gt;, even when OnClick is set, so a Removable
/// chip's own remove &lt;button&gt; never ends up nested inside another
/// &lt;button&gt;, which HTML disallows — click/keyboard activation on the chip
/// itself is wired up by hand instead (role="button", tabindex, Enter/Space).
/// </summary>
public sealed class FaChip : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public FaIconName? IconName { get; set; }
    [Parameter] public FaBadgeVariant Variant { get; set; } = FaBadgeVariant.Neutral;

    /// <summary>XSmall/Small/Medium(default)/Large/XLarge. See <see cref="FaSize"/>.</summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    [Parameter] public bool Selected { get; set; }
    [Parameter] public bool Removable { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public EventCallback OnRemove { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private bool IsInteractive => OnClick.HasDelegate && !Disabled;

    private string? VariantClass => Variant switch
    {
        FaBadgeVariant.Success => "fa-chip-success",
        FaBadgeVariant.Danger => "fa-chip-danger",
        FaBadgeVariant.Primary => "fa-chip-primary",
        _ => null
    };

    private Task HandleClickAsync() => IsInteractive ? OnClick.InvokeAsync() : Task.CompletedTask;

    private Task HandleKeyDownAsync(KeyboardEventArgs e) =>
        IsInteractive && e.Key is "Enter" or " " ? OnClick.InvokeAsync() : Task.CompletedTask;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", CssClassNames.Combine(
            "fa-chip",
            VariantClass,
            FaSizeClassNames.Class("fa-chip", Size),
            Selected ? "fa-chip-selected" : null,
            IsInteractive ? "fa-chip-clickable" : null,
            Disabled ? "fa-chip-disabled" : null,
            CssClass));

        if (IsInteractive)
        {
            builder.AddAttribute(2, "role", "button");
            builder.AddAttribute(3, "tabindex", "0");
            builder.AddAttribute(4, "aria-pressed", Selected ? "true" : "false");
            builder.AddAttribute(5, "onclick", EventCallback.Factory.Create(this, HandleClickAsync));
            builder.AddAttribute(6, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        }

        var seq = 10;
        if (IconName is { } icon)
        {
            builder.OpenComponent<FaIcon>(seq++);
            builder.AddComponentParameter(seq++, nameof(FaIcon.Name), icon);
            builder.AddComponentParameter(seq++, nameof(FaIcon.Color), FaIconColor.Black);
            builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 14);
            builder.CloseComponent();
        }

        builder.OpenElement(seq++, "span");
        builder.AddAttribute(seq++, "class", "fa-chip-label");
        builder.AddContent(seq++, ChildContent);
        builder.CloseElement();

        if (Removable)
        {
            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "class", "fa-chip-remove");
            builder.AddAttribute(seq++, "aria-label", "Remove");
            builder.AddAttribute(seq++, "disabled", Disabled);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async _ => await OnRemove.InvokeAsync()));
            builder.AddEventStopPropagationAttribute(seq++, "onclick", true);
            builder.OpenComponent<FaIcon>(seq++);
            builder.AddComponentParameter(seq++, nameof(FaIcon.Name), FaIconName.Cancel);
            builder.AddComponentParameter(seq++, nameof(FaIcon.Color), FaIconColor.Black);
            builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 10);
            builder.CloseComponent();
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
