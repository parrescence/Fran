using FaFa.Components;
using FaFa.Layout;
using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Templates;

/// <summary>
/// A single-form page: <see cref="FaStandardShell"/> (header + footer, no sidebar —
/// a focused form flow doesn't want app nav pulling attention away from it) wrapped
/// around one centered <see cref="FaCard"/>. <see cref="ChildContent"/> is your
/// actual form (typically an <c>&lt;EditForm&gt;</c> full of <c>FaInput</c>/
/// <c>FaSelect</c>/etc., or a plain <see cref="FaForm{TModel}"/>) — this template
/// only owns the page-level framing around it: the shell, the centered card, an
/// optional title/description, and an optional "back" link.
/// </summary>
public sealed class FaFormTemplate : ComponentBase
{
    [Parameter, EditorRequired] public string BrandText { get; set; } = "";
    [Parameter] public string BrandHref { get; set; } = "";

    /// <summary>Passed straight through to <see cref="FaStandardShell.BrandIconUrl"/>.</summary>
    [Parameter] public string? BrandIconUrl { get; set; }

    [Parameter] public bool IsAuthenticated { get; set; }
    [Parameter] public string? UserDisplayName { get; set; }
    [Parameter] public string? UserImageUrl { get; set; }
    [Parameter] public EventCallback OnLogin { get; set; }
    [Parameter] public EventCallback OnLogout { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>Passed straight through to <see cref="FaHeader.Position"/> — Standard (not sticky, the default), Sticky, or Floating.</summary>
    [Parameter] public FaNavPosition HeaderPosition { get; set; } = FaNavPosition.Standard;
    /// <summary>Passed straight through to <see cref="FaFooter.Position"/>.</summary>
    [Parameter] public FaNavPosition FooterPosition { get; set; } = FaNavPosition.Standard;

    /// <summary>Card heading, e.g. "Invite a teammate".</summary>
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Description { get; set; }
    /// <summary>Any valid CSS width — how wide the centered card gets on larger screens.</summary>
    [Parameter] public string MaxWidth { get; set; } = "28rem";
    /// <summary>Optional "‹ Back to orders" link above the card. Omit to skip it.</summary>
    [Parameter] public string? BackHref { get; set; }
    [Parameter] public string BackText { get; set; } = "Back";

    /// <summary>The form itself.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<FaStandardShell>(0);
        builder.AddComponentParameter(1, nameof(FaStandardShell.BrandText), BrandText);
        builder.AddComponentParameter(2, nameof(FaStandardShell.BrandHref), BrandHref);
        builder.AddComponentParameter(12, nameof(FaStandardShell.BrandIconUrl), BrandIconUrl);
        builder.AddComponentParameter(3, nameof(FaStandardShell.IsAuthenticated), IsAuthenticated);
        builder.AddComponentParameter(4, nameof(FaStandardShell.UserDisplayName), UserDisplayName);
        builder.AddComponentParameter(5, nameof(FaStandardShell.UserImageUrl), UserImageUrl);
        builder.AddComponentParameter(6, nameof(FaStandardShell.OnLogin), OnLogin);
        builder.AddComponentParameter(7, nameof(FaStandardShell.OnLogout), OnLogout);
        builder.AddComponentParameter(8, nameof(FaStandardShell.FooterContent), FooterContent);
        builder.AddComponentParameter(9, nameof(FaStandardShell.HeaderPosition), HeaderPosition);
        builder.AddComponentParameter(10, nameof(FaStandardShell.FooterPosition), FooterPosition);
        builder.AddComponentParameter(11, nameof(FaStandardShell.ChildContent), (RenderFragment)RenderBody);
        builder.CloseComponent();
    }

    private void RenderBody(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-template-form-wrap");
        builder.AddAttribute(2, "style", $"max-width:{MaxWidth}");

        if (!string.IsNullOrEmpty(BackHref))
        {
            builder.OpenElement(3, "a");
            builder.AddAttribute(4, "class", "fa-template-back-link");
            builder.AddAttribute(5, "href", BackHref);
            builder.AddContent(6, $"‹ {BackText}");
            builder.CloseElement();
        }

        builder.OpenComponent<FaCard>(7);
        builder.AddComponentParameter(8, nameof(FaCard.ChildContent), (RenderFragment)RenderCard);
        builder.CloseComponent();

        builder.CloseElement();
    }

    private void RenderCard(RenderTreeBuilder builder)
    {
        if (!string.IsNullOrEmpty(Title))
        {
            builder.OpenElement(0, "h2");
            builder.AddAttribute(1, "class", "fa-template-form-title");
            builder.AddContent(2, Title);
            builder.CloseElement();
        }

        if (!string.IsNullOrEmpty(Description))
        {
            builder.OpenElement(3, "p");
            builder.AddAttribute(4, "class", "fa-template-form-description");
            builder.AddContent(5, Description);
            builder.CloseElement();
        }

        builder.AddContent(6, ChildContent);
    }
}
