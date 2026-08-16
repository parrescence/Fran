using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

/// <summary>
/// A ready-made "are you sure you want to log out?" confirmation. No fields to
/// fill in — just a message and Confirm/Cancel buttons. FaLogoutForm never signs
/// anyone out itself; <see cref="OnConfirm"/> is where the caller actually clears
/// the session. Drop it inside an <see cref="FaModal"/> for the common "confirm in
/// a dialog" placement, or render it inline.
/// </summary>
public sealed class FaLogoutForm : ComponentBase
{
    [Parameter, EditorRequired] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public string Message { get; set; } = "Are you sure you want to log out?";
    [Parameter] public string ConfirmText { get; set; } = "Log out";
    [Parameter] public string CancelText { get; set; } = "Cancel";
    [Parameter] public bool Busy { get; set; }
    [Parameter] public FaAlign ButtonAlign { get; set; } = FaAlign.End;
    [Parameter] public string? CssClass { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-form fa-logout-form", CssClass));

        builder.OpenElement(2, "p");
        builder.AddAttribute(3, "class", "fa-logout-message");
        builder.AddContent(4, Message);
        builder.CloseElement();

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "class", CssClassNames.Combine("fa-form-actions", FaAlignClassNames.ToClass(ButtonAlign)));

        if (OnCancel.HasDelegate)
        {
            builder.OpenComponent<FaButton>(7);
            builder.AddComponentParameter(8, nameof(FaButton.Variant), FaButtonVariant.Outline);
            builder.AddComponentParameter(9, nameof(FaButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => OnCancel.InvokeAsync()));
            builder.AddComponentParameter(10, nameof(FaButton.ChildContent), (RenderFragment)(b => b.AddContent(0, CancelText)));
            builder.CloseComponent();
        }

        builder.OpenComponent<FaButton>(11);
        builder.AddComponentParameter(12, nameof(FaButton.Variant), FaButtonVariant.Danger);
        builder.AddComponentParameter(13, nameof(FaButton.Disabled), Busy);
        builder.AddComponentParameter(14, nameof(FaButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => OnConfirm.InvokeAsync()));
        builder.AddComponentParameter(15, nameof(FaButton.ChildContent), (RenderFragment)(b => b.AddContent(0, Busy ? "Logging out…" : ConfirmText)));
        builder.CloseComponent();

        builder.CloseElement();
        builder.CloseElement();
    }
}
