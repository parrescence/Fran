using FaFa.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Templates;

/// <summary>
/// A landing/overview page: <see cref="FaStandardShell"/> plus a hero band above
/// <see cref="ChildContent"/>'s own sections. The hero has a built-in default
/// (<see cref="HeroTitle"/>/<see cref="HeroDescription"/>/<see cref="HeroActions"/>,
/// typically one or two <c>FaButton</c>s) for the common case, but <see cref="Hero"/>
/// — a full <c>RenderFragment</c> — overrides it completely when the default band
/// isn't enough; set at most one of the two (Hero wins if both are set).
/// </summary>
public sealed class FaHomeTemplate : ComponentBase
{
    [Parameter, EditorRequired] public string BrandText { get; set; } = "";
    [Parameter] public string BrandHref { get; set; } = "";
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

    [Parameter] public string? HeroTitle { get; set; }
    [Parameter] public string? HeroDescription { get; set; }
    [Parameter] public RenderFragment? HeroActions { get; set; }
    /// <summary>Full replacement for the built-in hero band. Wins over HeroTitle/HeroDescription/HeroActions when set.</summary>
    [Parameter] public RenderFragment? Hero { get; set; }

    /// <summary>Whatever comes after the hero — feature sections, a pricing table, testimonials, etc.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<FaStandardShell>(0);
        builder.AddComponentParameter(1, nameof(FaStandardShell.BrandText), BrandText);
        builder.AddComponentParameter(2, nameof(FaStandardShell.BrandHref), BrandHref);
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
        if (Hero is not null)
        {
            builder.AddContent(0, Hero);
        }
        else if (!string.IsNullOrEmpty(HeroTitle))
        {
            builder.OpenElement(1, "section");
            builder.AddAttribute(2, "class", "fa-template-hero");

            builder.OpenElement(3, "h1");
            builder.AddContent(4, HeroTitle);
            builder.CloseElement();

            if (!string.IsNullOrEmpty(HeroDescription))
            {
                builder.OpenElement(5, "p");
                builder.AddContent(6, HeroDescription);
                builder.CloseElement();
            }

            if (HeroActions is not null)
            {
                builder.OpenElement(7, "div");
                builder.AddAttribute(8, "class", "fa-template-hero-actions");
                builder.AddContent(9, HeroActions);
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.AddContent(10, ChildContent);
    }
}
