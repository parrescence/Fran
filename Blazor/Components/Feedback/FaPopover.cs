using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

/// <summary>
/// Click-to-open contextual panel — a menu, a mini-form, extra detail that doesn't
/// belong inline. Same open/close pattern as FaDatePicker's calendar popup: a plain
/// Blazor bool, no JS interop, closing on Escape or on focus leaving the control via
/// a short grace-period @onfocusout (see FaDatePicker.cs's remarks for why a grace
/// period beats a bare focusout).
/// </summary>
public sealed class FaPopover : ComponentBase
{
    [Parameter, EditorRequired] public RenderFragment Trigger { get; set; } = default!;
    [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;
    [Parameter] public FaTooltipPosition Position { get; set; } = FaTooltipPosition.Bottom;
    [Parameter] public string? CssClass { get; set; }

    private bool _isOpen;
    private CancellationTokenSource? _pendingClose;

    private string PositionClass => Position switch
    {
        FaTooltipPosition.Top => "fa-popover-top",
        FaTooltipPosition.Left => "fa-popover-left",
        FaTooltipPosition.Right => "fa-popover-right",
        _ => "fa-popover-bottom"
    };

    private void Toggle()
    {
        CancelPendingClose();
        _isOpen = !_isOpen;
    }

    private void HandleFocusIn() => CancelPendingClose();

    private async Task HandleFocusOutAsync()
    {
        CancelPendingClose();
        _pendingClose = new CancellationTokenSource();
        var token = _pendingClose.Token;
        try
        {
            await Task.Delay(150, token);
            _isOpen = false;
            StateHasChanged();
        }
        catch (TaskCanceledException)
        {
            // A focusin inside the panel/trigger cancelled the close — nothing to do.
        }
    }

    private void CancelPendingClose()
    {
        _pendingClose?.Cancel();
        _pendingClose = null;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            _isOpen = false;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-popover", CssClass));
        builder.AddAttribute(2, "onfocusin", EventCallback.Factory.Create(this, HandleFocusIn));
        builder.AddAttribute(3, "onfocusout", EventCallback.Factory.Create(this, HandleFocusOutAsync));
        builder.AddAttribute(4, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDown));

        builder.OpenElement(5, "button");
        builder.AddAttribute(6, "type", "button");
        builder.AddAttribute(7, "class", "fa-popover-trigger");
        builder.AddAttribute(8, "aria-expanded", _isOpen ? "true" : "false");
        builder.AddAttribute(9, "onclick", EventCallback.Factory.Create(this, Toggle));
        builder.AddContent(10, Trigger);
        builder.CloseElement();

        builder.OpenElement(11, "div");
        builder.AddAttribute(12, "class", CssClassNames.Combine("fa-popover-panel", PositionClass, _isOpen ? "fa-popover-panel-open" : null));
        builder.AddContent(13, ChildContent);
        builder.CloseElement();

        builder.CloseElement();
    }
}
