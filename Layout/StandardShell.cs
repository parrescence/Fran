using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Layout;

/// <summary>
/// Template 1: header + content + footer, no sidebar. For pages that don't need app
/// navigation alongside them (landing/marketing pages, standalone flows).
/// </summary>
public sealed class StandardShell : ComponentBase
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-shell fa-shell-standard");

        builder.OpenComponent<AppHeader>(2);
        builder.AddComponentParameter(3, nameof(AppHeader.BrandText), BrandText);
        builder.AddComponentParameter(4, nameof(AppHeader.BrandHref), BrandHref);
        builder.AddComponentParameter(5, nameof(AppHeader.IsAuthenticated), IsAuthenticated);
        builder.AddComponentParameter(6, nameof(AppHeader.UserDisplayName), UserDisplayName);
        builder.AddComponentParameter(7, nameof(AppHeader.UserImageUrl), UserImageUrl);
        builder.AddComponentParameter(8, nameof(AppHeader.OnLogin), OnLogin);
        builder.AddComponentParameter(9, nameof(AppHeader.OnLogout), OnLogout);
        builder.CloseComponent();

        builder.OpenElement(10, "main");
        builder.AddAttribute(11, "class", "fa-shell-main");
        builder.AddContent(12, ChildContent);
        builder.CloseElement();

        builder.OpenComponent<AppFooter>(13);
        builder.AddComponentParameter(14, nameof(AppFooter.BrandText), BrandText);
        builder.AddComponentParameter(15, nameof(AppFooter.ChildContent), FooterContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}
