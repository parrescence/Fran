using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Layout;

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

    /// <summary>Passed straight through to <see cref="FaHeader.Position"/>.</summary>
    [Parameter] public FaNavPosition HeaderPosition { get; set; } = FaNavPosition.Standard;

    /// <summary>Passed straight through to <see cref="FaFooter.Position"/>.</summary>
    [Parameter] public FaNavPosition FooterPosition { get; set; } = FaNavPosition.Standard;

    /// <summary>Passed straight through to <see cref="FaSidebar.Position"/>.</summary>
    [Parameter] public FaNavPosition SidebarPosition { get; set; } = FaNavPosition.Standard;

    /// <summary>Passed straight through to <see cref="FaSidebar.Collapsible"/>.</summary>
    [Parameter] public bool SidebarCollapsible { get; set; }

    /// <summary>
    /// Opt-in, off by default. Pins the header, sidebar, and footer to the viewport
    /// edges and lets only <see cref="ChildContent"/> (inside &lt;main&gt;) scroll
    /// internally, instead of the whole page growing past one viewport and scrolling
    /// as a single document (the default — fine for most content, but wrong for a
    /// dashboard-style page that wants the chrome pinned). A too-tall Sidebar scrolls
    /// independently too, same as it already does under Sticky/Floating. See
    /// <c>_layout.scss</c>'s <c>.fa-shell-contained</c> for the mechanics.
    /// </summary>
    [Parameter] public bool ContainScroll { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-shell", "fa-shell-sidebar", ContainScroll ? "fa-shell-contained" : null));

        builder.OpenComponent<FaHeader>(2);
        builder.AddComponentParameter(3, nameof(FaHeader.BrandText), BrandText);
        builder.AddComponentParameter(4, nameof(FaHeader.BrandHref), BrandHref);
        builder.AddComponentParameter(5, nameof(FaHeader.IsAuthenticated), IsAuthenticated);
        builder.AddComponentParameter(6, nameof(FaHeader.UserDisplayName), UserDisplayName);
        builder.AddComponentParameter(7, nameof(FaHeader.UserImageUrl), UserImageUrl);
        builder.AddComponentParameter(8, nameof(FaHeader.OnLogin), OnLogin);
        builder.AddComponentParameter(9, nameof(FaHeader.OnLogout), OnLogout);
        builder.AddComponentParameter(20, nameof(FaHeader.Position), HeaderPosition);
        builder.AddComponentParameter(26, nameof(FaHeader.ShowSidebarToggle), true);
        builder.CloseComponent();

        builder.OpenElement(10, "div");
        builder.AddAttribute(11, "class", "fa-shell-body");

        builder.OpenComponent<FaSidebar>(12);
        builder.AddComponentParameter(13, nameof(FaSidebar.ChildContent), Sidebar);
        builder.AddComponentParameter(21, nameof(FaSidebar.Position), SidebarPosition);
        builder.AddComponentParameter(25, nameof(FaSidebar.Collapsible), SidebarCollapsible);
        builder.CloseComponent();

        // main + footer share this column (rather than footer sitting after
        // .fa-shell-body, as a sibling of it) so a Sticky/Floating sidebar's
        // containing block spans both — its sticky containment ends at the bottom
        // of the footer, not the moment <main>'s own content runs out. Without this,
        // the sidebar got shoved upward the instant main content ended, well before
        // the actual bottom of the page, on any page shorter than a couple of
        // viewports tall.
        builder.OpenElement(23, "div");
        builder.AddAttribute(24, "class", "fa-shell-main-col");

        builder.OpenElement(14, "main");
        builder.AddAttribute(15, "class", "fa-shell-main");
        builder.AddContent(16, ChildContent);
        builder.CloseElement();

        builder.OpenComponent<FaFooter>(17);
        builder.AddComponentParameter(18, nameof(FaFooter.BrandText), BrandText);
        builder.AddComponentParameter(19, nameof(FaFooter.ChildContent), FooterContent);
        builder.AddComponentParameter(22, nameof(FaFooter.Position), FooterPosition);
        builder.CloseComponent();

        builder.CloseElement(); // .fa-shell-main-col

        builder.CloseElement(); // .fa-shell-body

        builder.CloseElement();
    }
}
