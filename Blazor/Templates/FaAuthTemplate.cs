using FaFa.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Templates;

/// <summary>
/// A chrome-free auth page: no <c>FaHeader</c>/<c>FaSidebar</c>/<c>FaFooter</c> at
/// all — a login/register/reset-password page wants the visitor's full attention on
/// the one card, not app navigation they can't use yet anyway. Just a brand link,
/// a centered <see cref="FaCard"/>, and an optional small line of content below it
/// (a "Don't have an account? Sign up" link, typically). Pair with
/// <c>FaLoginForm</c>/<c>FaLogoutForm</c> or your own <c>EditForm</c> as
/// <see cref="ChildContent"/>. Reach for <see cref="FaFormTemplate"/> instead when
/// the page should keep the site's normal header/footer around the form.
/// </summary>
public sealed class FaAuthTemplate : ComponentBase
{
    [Parameter, EditorRequired] public string BrandText { get; set; } = "";
    [Parameter] public string BrandHref { get; set; } = "";

    /// <summary>
    /// Optional logo/icon shown to the left of <see cref="BrandText"/> — same
    /// parameter/rendering as <see cref="FaFa.Layout.FaHeader.BrandIconUrl"/>, since
    /// this template renders its own brand link rather than going through
    /// <c>FaHeader</c> (see this class's own doc comment for why).
    /// </summary>
    [Parameter] public string? BrandIconUrl { get; set; }

    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Description { get; set; }
    /// <summary>Any valid CSS width — how wide the centered card gets on larger screens.</summary>
    [Parameter] public string MaxWidth { get; set; } = "24rem";

    /// <summary>The form itself.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }
    /// <summary>Small content below the card — a "Sign up instead" link, terms text, etc.</summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-template-auth");

        builder.OpenElement(2, "a");
        builder.AddAttribute(3, "class", "fa-template-auth-brand");
        builder.AddAttribute(4, "href", BrandHref);

        if (!string.IsNullOrEmpty(BrandIconUrl))
        {
            builder.OpenElement(14, "img");
            builder.AddAttribute(15, "class", "fa-template-auth-brand-icon");
            builder.AddAttribute(16, "src", BrandIconUrl);
            builder.AddAttribute(17, "alt", "");
            builder.CloseElement();
        }

        builder.AddContent(5, BrandText);
        builder.CloseElement();

        builder.OpenElement(6, "div");
        builder.AddAttribute(7, "class", "fa-template-auth-card-wrap");
        builder.AddAttribute(8, "style", $"max-width:{MaxWidth}");

        builder.OpenComponent<FaCard>(9);
        builder.AddComponentParameter(10, nameof(FaCard.ChildContent), (RenderFragment)RenderCard);
        builder.CloseComponent();

        if (FooterContent is not null)
        {
            builder.OpenElement(11, "div");
            builder.AddAttribute(12, "class", "fa-template-auth-footer");
            builder.AddContent(13, FooterContent);
            builder.CloseElement();
        }

        builder.CloseElement();
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
