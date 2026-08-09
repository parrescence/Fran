using FactoryAspects.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

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

    /// <summary>
    /// Whether clicking the backdrop (outside the dialog) invokes <see cref="OnClose"/>.
    /// Defaults to <c>true</c>. Set <c>false</c> for flows that must stay open until the
    /// caller performs an explicit close action (the header's close button or a button in
    /// <see cref="FooterContent"/> invoking <see cref="OnClose"/> itself) — e.g. a form the
    /// user shouldn't be able to dismiss by an accidental outside click.
    /// </summary>
    [Parameter] public bool CloseOnBackdropClick { get; set; } = true;

    /// <summary>
    /// Whether <see cref="ChildContent"/> is torn down and freshly re-created the next time
    /// the modal opens, discarding whatever state its elements picked up while it was open
    /// (uncontrolled input values, a form's local edit state, ...). Defaults to <c>false</c>
    /// — content stays mounted (just hidden) across a close/reopen cycle so its values
    /// persist. Set <c>true</c> for flows that should always start from a clean slate.
    /// </summary>
    [Parameter] public bool ResetOnClose { get; set; }

    private Task HandleBackdropClick() =>
        CloseOnBackdropClick ? OnClose.InvokeAsync() : Task.CompletedTask;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Show && ResetOnClose)
        {
            return;
        }

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ClassNames.Combine("fa-modal-backdrop", Show ? null : "fa-modal-backdrop-hidden"));
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, HandleBackdropClick));
        if (!Show)
        {
            builder.AddAttribute(3, "aria-hidden", "true");
        }

        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", "fa-modal");
        builder.AddEventStopPropagationAttribute(6, "onclick", true);
        builder.AddAttribute(8, "role", "dialog");
        builder.AddAttribute(9, "aria-modal", "true");

        builder.OpenElement(10, "div");
        builder.AddAttribute(11, "class", "fa-modal-header");
        builder.OpenElement(12, "h5");
        builder.AddAttribute(13, "class", "fa-modal-title");
        builder.AddContent(14, Title);
        builder.CloseElement();
        builder.OpenElement(15, "button");
        builder.AddAttribute(16, "type", "button");
        builder.AddAttribute(17, "class", "fa-modal-close");
        builder.AddAttribute(18, "aria-label", "Close");
        builder.AddAttribute(19, "onclick", EventCallback.Factory.Create(this, () => OnClose.InvokeAsync()));
        builder.AddContent(20, "×");
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(21, "div");
        builder.AddAttribute(22, "class", "fa-modal-body");
        builder.AddContent(23, ChildContent);
        builder.CloseElement();

        if (FooterContent is not null)
        {
            builder.OpenElement(24, "div");
            builder.AddAttribute(25, "class", "fa-modal-footer");
            builder.AddContent(26, FooterContent);
            builder.CloseElement();
        }

        builder.CloseElement();
        builder.CloseElement();
    }
}
