using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// Backdrop-covered dialog — first reusable modal in the app (prior "create X" flows
/// were inline page forms). Backdrop click and the header's close button both invoke
/// <see cref="OnClose"/>; the caller owns whether it's currently visible (no internal
/// show/hide state) so it can gate the close behind unsaved-changes checks if it ever
/// needs to.
/// </summary>
public sealed class FaModal : ComponentBase
{
    [Parameter] public bool Show { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    private Task HandleBackdropClick() => OnClose.InvokeAsync();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Show)
        {
            return;
        }

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-modal-backdrop");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, HandleBackdropClick));

        builder.OpenElement(3, "div");
        builder.AddAttribute(4, "class", "fa-modal");
        builder.AddAttribute(5, "onclick:stopPropagation", true);
        builder.AddAttribute(7, "role", "dialog");
        builder.AddAttribute(8, "aria-modal", "true");

        builder.OpenElement(9, "div");
        builder.AddAttribute(10, "class", "fa-modal-header");
        builder.OpenElement(11, "h5");
        builder.AddAttribute(12, "class", "fa-modal-title");
        builder.AddContent(13, Title);
        builder.CloseElement();
        builder.OpenElement(14, "button");
        builder.AddAttribute(15, "type", "button");
        builder.AddAttribute(16, "class", "fa-modal-close");
        builder.AddAttribute(17, "aria-label", "Close");
        builder.AddAttribute(18, "onclick", EventCallback.Factory.Create(this, () => OnClose.InvokeAsync()));
        builder.AddContent(19, "×");
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(20, "div");
        builder.AddAttribute(21, "class", "fa-modal-body");
        builder.AddContent(22, ChildContent);
        builder.CloseElement();

        if (FooterContent is not null)
        {
            builder.OpenElement(23, "div");
            builder.AddAttribute(24, "class", "fa-modal-footer");
            builder.AddContent(25, FooterContent);
            builder.CloseElement();
        }

        builder.CloseElement();
        builder.CloseElement();
    }
}
