using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Fran.Components;

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

    /// <summary>Where the dialog sits within the backdrop. Defaults to centered.</summary>
    [Parameter] public FaModalPosition Position { get; set; } = FaModalPosition.Center;

    /// <summary>The dialog's max-width. Defaults to Medium — FaModal's original, only size before this parameter existed.</summary>
    [Parameter] public FaModalSize Size { get; set; } = FaModalSize.Medium;

    /// <summary>justify-content for the footer's button row. Defaults to End (flex-end) — FaModal's original, only alignment before this parameter existed.</summary>
    [Parameter] public FaAlign FooterAlign { get; set; } = FaAlign.End;

    /// <summary>
    /// Whether the header's × button shows. Defaults to <c>true</c>. Set <c>false</c>
    /// for a modal that can only be closed via a button inside <see
    /// cref="FooterContent"/> invoking <see cref="OnClose"/> itself (or, if <see
    /// cref="CloseOnBackdropClick"/> is also left <c>true</c>, by clicking outside it)
    /// — e.g. when the header's real estate is better spent on something else and the
    /// footer already has an explicit "Cancel"/"Close" action.
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Whether clicking the backdrop (outside the dialog) invokes <see cref="OnClose"/>.
    /// Defaults to <c>true</c>. Set <c>false</c> for flows that must stay open until the
    /// caller performs an explicit close action (the header's close button or a button in
    /// <see cref="FooterContent"/> invoking <see cref="OnClose"/> itself) — e.g. a form the
    /// user shouldn't be able to dismiss by an accidental outside click.
    /// </summary>
    [Parameter] public bool CloseOnBackdropClick { get; set; } = true;

    /// <summary>
    /// Whether a loading overlay covers the dialog (header/body/footer alike).
    /// Defaults to <c>false</c> — off unless a caller opts in, e.g. while an async
    /// submit is in flight. The caller owns the flag (no internal state), same as
    /// <see cref="Show"/> itself.
    /// </summary>
    [Parameter] public bool IsLoading { get; set; }

    /// <summary>
    /// What to render inside the loading overlay when <see cref="IsLoading"/> is
    /// <c>true</c>. Defaults to <see cref="FaHelixLoader"/> when left unset — pass
    /// this to swap in an <see cref="FaSpinner"/>, custom text, or anything else.
    /// </summary>
    [Parameter] public RenderFragment? LoadingContent { get; set; }

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

    private string? PositionClass => Position switch
    {
        FaModalPosition.Top => "fa-modal-backdrop-top",
        FaModalPosition.Bottom => "fa-modal-backdrop-bottom",
        FaModalPosition.Left => "fa-modal-backdrop-left",
        FaModalPosition.Right => "fa-modal-backdrop-right",
        _ => null
    };

    private string? SizeClass => Size switch
    {
        FaModalSize.Small => "fa-modal-sm",
        FaModalSize.Large => "fa-modal-lg",
        _ => null
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Show && ResetOnClose)
        {
            return;
        }

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-modal-backdrop", PositionClass, Show ? null : "fa-modal-backdrop-hidden"));
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, HandleBackdropClick));
        if (!Show)
        {
            builder.AddAttribute(3, "aria-hidden", "true");
        }

        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "class", CssClassNames.Combine("fa-modal", SizeClass));
        builder.AddEventStopPropagationAttribute(6, "onclick", true);
        builder.AddAttribute(8, "role", "dialog");
        builder.AddAttribute(9, "aria-modal", "true");

        builder.OpenElement(10, "div");
        builder.AddAttribute(11, "class", "fa-modal-header");
        builder.OpenElement(12, "h5");
        builder.AddAttribute(13, "class", "fa-modal-title");
        builder.AddContent(14, Title);
        builder.CloseElement();
        if (ShowCloseButton)
        {
            builder.OpenElement(15, "button");
            builder.AddAttribute(16, "type", "button");
            builder.AddAttribute(17, "class", "fa-modal-close");
            builder.AddAttribute(18, "aria-label", "Close");
            builder.AddAttribute(19, "onclick", EventCallback.Factory.Create(this, () => OnClose.InvokeAsync()));
            builder.AddContent(20, "×");
            builder.CloseElement();
        }
        builder.CloseElement();

        builder.OpenElement(21, "div");
        builder.AddAttribute(22, "class", "fa-modal-body");
        builder.AddContent(23, ChildContent);
        builder.CloseElement();

        if (FooterContent is not null)
        {
            builder.OpenElement(24, "div");
            builder.AddAttribute(25, "class", CssClassNames.Combine("fa-modal-footer", FaAlignClassNames.ToClass(FooterAlign)));
            builder.AddContent(26, FooterContent);
            builder.CloseElement();
        }

        // Overlay sits on top of the whole dialog (header/body/footer alike) rather
        // than swapping in for it, same "cover, don't replace" behavior FaGrid's own
        // loading overlay uses.
        if (IsLoading)
        {
            builder.OpenElement(27, "div");
            builder.AddAttribute(28, "class", "fa-modal-loading-overlay");
            if (LoadingContent is not null)
            {
                builder.AddContent(29, LoadingContent);
            }
            else
            {
                builder.OpenComponent<FaHelixLoader>(30);
                builder.CloseComponent();
            }
            builder.CloseElement();
        }

        builder.CloseElement();
        builder.CloseElement();
    }
}
