using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// Mount once (e.g. in your root layout) to render every injected FaToastService's
/// Show(...) call as a stacked, auto-dismissing toast. Requires
/// <c>builder.Services.AddScoped&lt;FaToastService&gt;();</c> in your Program.cs —
/// see fa-toast.md. Each active toast gets a stable incrementing int id — not a
/// Guid, since nothing outside this component ever needs to reference one by id,
/// just something stable for @key so a toast that's expiring doesn't get confused
/// with the one that replaces it in the list.
/// </summary>
public sealed class FaToastHost : ComponentBase, IDisposable
{
    [Inject] private FaToastService ToastService { get; set; } = default!;

    [Parameter] public FaToastPosition Position { get; set; } = FaToastPosition.TopRight;

    private sealed record ActiveToast(int Id, FaToastMessage Message);

    private readonly List<ActiveToast> _toasts = new();
    private int _nextId;

    protected override void OnInitialized()
    {
        ToastService.OnShow += HandleShow;
    }

    private void HandleShow(FaToastMessage message)
    {
        var toast = new ActiveToast(_nextId++, message);
        _toasts.Add(toast);
        InvokeAsync(StateHasChanged);
        _ = DismissAfterAsync(toast);
    }

    private async Task DismissAfterAsync(ActiveToast toast)
    {
        await Task.Delay(toast.Message.DurationMs);
        if (_toasts.Remove(toast))
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private void Dismiss(int id)
    {
        _toasts.RemoveAll(t => t.Id == id);
    }

    private string PositionClass => Position switch
    {
        FaToastPosition.TopLeft => "fa-toast-host-top-left",
        FaToastPosition.TopCenter => "fa-toast-host-top-center",
        FaToastPosition.BottomRight => "fa-toast-host-bottom-right",
        FaToastPosition.BottomLeft => "fa-toast-host-bottom-left",
        FaToastPosition.BottomCenter => "fa-toast-host-bottom-center",
        _ => "fa-toast-host-top-right"
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-toast-host", PositionClass));
        builder.AddAttribute(2, "aria-live", "polite");

        foreach (var toast in _toasts)
        {
            var id = toast.Id;
            var variantClass = toast.Message.Variant switch
            {
                FaAlertVariant.Success => "fa-toast-success",
                FaAlertVariant.Danger => "fa-toast-danger",
                _ => "fa-toast-info"
            };

            builder.OpenElement(3, "div");
            builder.SetKey(id);
            builder.AddAttribute(4, "class", CssClassNames.Combine("fa-toast", variantClass));
            builder.AddAttribute(5, "role", "status");

            builder.OpenElement(6, "span");
            builder.AddAttribute(7, "class", "fa-toast-text");
            builder.AddContent(8, toast.Message.Text);
            builder.CloseElement();

            builder.OpenElement(9, "button");
            builder.AddAttribute(10, "type", "button");
            builder.AddAttribute(11, "class", "fa-toast-close");
            builder.AddAttribute(12, "aria-label", "Dismiss");
            builder.AddAttribute(13, "onclick", EventCallback.Factory.Create(this, () => Dismiss(id)));
            builder.AddContent(14, "×");
            builder.CloseElement();

            builder.CloseElement();
        }

        builder.CloseElement();
    }

    public void Dispose()
    {
        ToastService.OnShow -= HandleShow;
    }
}
