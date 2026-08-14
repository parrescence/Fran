using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Layout;

/// <summary>
/// Template 2: header + left sidebar + content + footer. Example consumer
/// FinanceApp.Web's MainLayout uses this template — Sidebar receives the app's own
/// NavMenu as content.
/// </summary>
public sealed class FaSidebarShell : ComponentBase
{
    [Parameter, EditorRequired] public string BrandText { get; set; } = "";
    [Parameter] public string BrandHref { get; set; } = "";
    [Parameter] public bool IsAuthenticated { get; set; }
    [Parameter] public string? UserDisplayName { get; set; }
    [Parameter] public string? UserImageUrl { get; set; }
    [Parameter] public EventCallback OnLogin { get; set; }
    [Parameter] public EventCallback OnLogout { get; set; }
    [Parameter] public RenderFragment? Sidebar { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-shell fa-shell-sidebar");

        builder.OpenComponent<FaHeader>(2);
        builder.AddComponentParameter(3, nameof(FaHeader.BrandText), BrandText);
        builder.AddComponentParameter(4, nameof(FaHeader.BrandHref), BrandHref);
        builder.AddComponentParameter(5, nameof(FaHeader.IsAuthenticated), IsAuthenticated);
        builder.AddComponentParameter(6, nameof(FaHeader.UserDisplayName), UserDisplayName);
        builder.AddComponentParameter(7, nameof(FaHeader.UserImageUrl), UserImageUrl);
        builder.AddComponentParameter(8, nameof(FaHeader.OnLogin), OnLogin);
        builder.AddComponentParameter(9, nameof(FaHeader.OnLogout), OnLogout);
        builder.CloseComponent();

        builder.OpenElement(10, "div");
        builder.AddAttribute(11, "class", "fa-shell-body");

        builder.OpenComponent<FaSidebar>(12);
        builder.AddComponentParameter(13, nameof(FaSidebar.ChildContent), Sidebar);
        builder.CloseComponent();

        builder.OpenElement(14, "main");
        builder.AddAttribute(15, "class", "fa-shell-main");
        builder.AddContent(16, ChildContent);
        builder.CloseElement();

        builder.CloseElement();

        builder.OpenComponent<FaFooter>(17);
        builder.AddComponentParameter(18, nameof(FaFooter.BrandText), BrandText);
        builder.AddComponentParameter(19, nameof(FaFooter.ChildContent), FooterContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}
