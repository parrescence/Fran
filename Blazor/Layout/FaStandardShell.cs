using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Layout;

/// <summary>
/// Template 1: header + content + footer, no sidebar. For pages that don't need app
/// navigation alongside them (landing/marketing pages, standalone flows).
/// </summary>
public sealed class FaStandardShell : ComponentBase
{
    [Parameter, EditorRequired] public string BrandText { get; set; } = "";
    [Parameter] public string BrandHref { get; set; } = "";
    [Parameter] public bool IsAuthenticated { get; set; }
    [Parameter] public string? UserDisplayName { get; set; }
    [Parameter] public string? UserImageUrl { get; set; }
    [Parameter] public EventCallback OnLogin { get; set; }
    [Parameter] public EventCallback OnLogout { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>Passed straight through to <see cref="FaHeader.Position"/>.</summary>
    [Parameter] public FaNavPosition HeaderPosition { get; set; } = FaNavPosition.Standard;

    /// <summary>Passed straight through to <see cref="FaFooter.Position"/>.</summary>
    [Parameter] public FaNavPosition FooterPosition { get; set; } = FaNavPosition.Standard;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-shell fa-shell-standard");

        builder.OpenComponent<FaHeader>(2);
        builder.AddComponentParameter(3, nameof(FaHeader.BrandText), BrandText);
        builder.AddComponentParameter(4, nameof(FaHeader.BrandHref), BrandHref);
        builder.AddComponentParameter(5, nameof(FaHeader.IsAuthenticated), IsAuthenticated);
        builder.AddComponentParameter(6, nameof(FaHeader.UserDisplayName), UserDisplayName);
        builder.AddComponentParameter(7, nameof(FaHeader.UserImageUrl), UserImageUrl);
        builder.AddComponentParameter(8, nameof(FaHeader.OnLogin), OnLogin);
        builder.AddComponentParameter(9, nameof(FaHeader.OnLogout), OnLogout);
        builder.AddComponentParameter(20, nameof(FaHeader.Position), HeaderPosition);
        builder.CloseComponent();

        builder.OpenElement(10, "main");
        builder.AddAttribute(11, "class", "fa-shell-main");
        builder.AddContent(12, ChildContent);
        builder.CloseElement();

        builder.OpenComponent<FaFooter>(13);
        builder.AddComponentParameter(14, nameof(FaFooter.BrandText), BrandText);
        builder.AddComponentParameter(15, nameof(FaFooter.ChildContent), FooterContent);
        builder.AddComponentParameter(21, nameof(FaFooter.Position), FooterPosition);
        builder.CloseComponent();

        builder.CloseElement();
    }
}
