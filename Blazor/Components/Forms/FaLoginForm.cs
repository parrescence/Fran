using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Fran.Components;

/// <summary>
/// A ready-made username/password login form. Plain HTML form + native inputs
/// (not <c>InputBase</c>/<c>EditForm</c>-based — there's no model for a caller to
/// own, so there's nothing an EditContext would add here) that owns its own
/// <see cref="FaLoginRequest"/> internally and hands it to <see cref="OnSubmit"/>
/// once the user submits. FaLoginForm never calls an auth endpoint itself — set
/// <see cref="ErrorText"/> from your own OnSubmit handler to show a failed-login
/// message, and <see cref="Busy"/> while that handler's await is in flight.
/// </summary>
public sealed class FaLoginForm : ComponentBase
{
    [Parameter, EditorRequired] public EventCallback<FaLoginRequest> OnSubmit { get; set; }
    [Parameter] public bool ShowRememberMe { get; set; } = true;
    [Parameter] public bool Busy { get; set; }
    [Parameter] public string? ErrorText { get; set; }
    [Parameter] public string UsernameLabel { get; set; } = "Username";
    [Parameter] public string PasswordLabel { get; set; } = "Password";
    [Parameter] public string SubmitText { get; set; } = "Log in";
    /// <summary>Extra content rendered between the fields and the submit button — a "Forgot password?" link, a sign-up link, etc.</summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private readonly FaLoginRequest _model = new();
    private readonly string _usernameId = $"fa-login-username-{Guid.NewGuid():N}";
    private readonly string _passwordId = $"fa-login-password-{Guid.NewGuid():N}";

    private Task HandleSubmit() => OnSubmit.InvokeAsync(_model);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "form");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-form fa-login-form", CssClass));
        builder.AddAttribute(2, "onsubmit", EventCallback.Factory.Create(this, HandleSubmit));
        builder.AddEventPreventDefaultAttribute(3, "onsubmit", true);

        var seq = 4;

        if (!string.IsNullOrEmpty(ErrorText))
        {
            builder.OpenComponent<FaAlert>(seq++);
            builder.AddComponentParameter(seq++, nameof(FaAlert.Variant), FaAlertVariant.Danger);
            builder.AddComponentParameter(seq++, nameof(FaAlert.ChildContent), (RenderFragment)(b => b.AddContent(0, ErrorText)));
            builder.CloseComponent();
        }

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-field");
        builder.OpenElement(seq++, "label");
        builder.AddAttribute(seq++, "class", "fa-label");
        builder.AddAttribute(seq++, "for", _usernameId);
        builder.AddContent(seq++, UsernameLabel);
        builder.CloseElement();
        builder.OpenElement(seq++, "input");
        builder.AddAttribute(seq++, "id", _usernameId);
        builder.AddAttribute(seq++, "class", "fa-input");
        builder.AddAttribute(seq++, "type", "text");
        builder.AddAttribute(seq++, "autocomplete", "username");
        builder.AddAttribute(seq++, "required", true);
        builder.AddAttribute(seq++, "value", _model.Username);
        builder.AddAttribute(seq++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e => _model.Username = e.Value?.ToString() ?? ""));
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-field");
        builder.OpenElement(seq++, "label");
        builder.AddAttribute(seq++, "class", "fa-label");
        builder.AddAttribute(seq++, "for", _passwordId);
        builder.AddContent(seq++, PasswordLabel);
        builder.CloseElement();
        builder.OpenElement(seq++, "input");
        builder.AddAttribute(seq++, "id", _passwordId);
        builder.AddAttribute(seq++, "class", "fa-input");
        builder.AddAttribute(seq++, "type", "password");
        builder.AddAttribute(seq++, "autocomplete", "current-password");
        builder.AddAttribute(seq++, "required", true);
        builder.AddAttribute(seq++, "value", _model.Password);
        builder.AddAttribute(seq++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e => _model.Password = e.Value?.ToString() ?? ""));
        builder.CloseElement();
        builder.CloseElement();

        if (ShowRememberMe)
        {
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "class", "fa-checkbox");
            builder.OpenElement(seq++, "input");
            builder.AddAttribute(seq++, "type", "checkbox");
            builder.AddAttribute(seq++, "class", "fa-checkbox-input");
            builder.AddAttribute(seq++, "checked", _model.RememberMe);
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => _model.RememberMe = e.Value is bool b && b));
            builder.CloseElement();
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "fa-checkbox-label");
            builder.AddContent(seq++, "Remember me");
            builder.CloseElement();
            builder.CloseElement();
        }

        if (FooterContent is not null)
        {
            builder.AddContent(seq++, FooterContent);
        }

        builder.OpenComponent<FaButton>(seq++);
        builder.AddComponentParameter(seq++, nameof(FaButton.Type), "submit");
        builder.AddComponentParameter(seq++, nameof(FaButton.Disabled), Busy);
        builder.AddComponentParameter(seq++, nameof(FaButton.ChildContent), (RenderFragment)(b => b.AddContent(0, Busy ? "Logging in…" : SubmitText)));
        builder.CloseComponent();

        builder.CloseElement();
    }
}
