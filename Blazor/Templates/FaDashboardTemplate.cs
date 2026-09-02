using FaFa.Layout;
using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Templates;

/// <summary>
/// A full dashboard page: <see cref="FaSidebarShell"/> (header + sidebar + footer)
/// plus the one thing every dashboard page repeats that the bare shell doesn't
/// provide — a title/actions row above the body content (e.g. "Overview" with a
/// "New report" button on the right). Everything shell-level (brand, auth, the
/// sidebar's own nav links, header/sidebar/footer <see cref="FaNavPosition"/>) is a
/// plain pass-through parameter, same names as <see cref="FaSidebarShell"/> itself,
/// so switching between using the shell directly and this template is a rename, not
/// a rewrite.
/// </summary>
public sealed class FaDashboardTemplate : ComponentBase
{
    [Parameter, EditorRequired] public string BrandText { get; set; } = "";
    [Parameter] public string BrandHref { get; set; } = "";

    /// <summary>Passed straight through to <see cref="FaSidebarShell.BrandIconUrl"/>.</summary>
    [Parameter] public string? BrandIconUrl { get; set; }

    [Parameter] public bool IsAuthenticated { get; set; }
    [Parameter] public string? UserDisplayName { get; set; }
    [Parameter] public string? UserImageUrl { get; set; }
    [Parameter] public EventCallback OnLogin { get; set; }
    [Parameter] public EventCallback OnLogout { get; set; }

    /// <summary>The sidebar's nav content — usually your app's own nav-menu component.</summary>
    [Parameter] public RenderFragment? Sidebar { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>Passed straight through to <see cref="FaHeader.Position"/> — Standard (not sticky, the default), Sticky, or Floating.</summary>
    [Parameter] public FaNavPosition HeaderPosition { get; set; } = FaNavPosition.Standard;
    /// <summary>Passed straight through to <see cref="FaFooter.Position"/>.</summary>
    [Parameter] public FaNavPosition FooterPosition { get; set; } = FaNavPosition.Standard;
    /// <summary>Passed straight through to <see cref="FaSidebar.Position"/>.</summary>
    [Parameter] public FaNavPosition SidebarPosition { get; set; } = FaNavPosition.Standard;
    /// <summary>Passed straight through to <see cref="FaSidebar.Collapsible"/>.</summary>
    [Parameter] public bool SidebarCollapsible { get; set; }
    /// <summary>Passed straight through to <see cref="FaSidebarShell.ContainScroll"/>.</summary>
    [Parameter] public bool ContainScroll { get; set; }

    /// <summary>Page heading, shown above <see cref="ChildContent"/>. Omit both this and <see cref="Actions"/> to skip the header row entirely.</summary>
    [Parameter] public string? Title { get; set; }
    /// <summary>Right-aligned content next to <see cref="Title"/> — typically one or two <c>FaButton</c>s.</summary>
    [Parameter] public RenderFragment? Actions { get; set; }

    /// <summary>The dashboard body — stat cards, a grid, charts, whatever the page actually shows.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<FaSidebarShell>(0);
        builder.AddComponentParameter(1, nameof(FaSidebarShell.BrandText), BrandText);
        builder.AddComponentParameter(2, nameof(FaSidebarShell.BrandHref), BrandHref);
        builder.AddComponentParameter(16, nameof(FaSidebarShell.BrandIconUrl), BrandIconUrl);
        builder.AddComponentParameter(3, nameof(FaSidebarShell.IsAuthenticated), IsAuthenticated);
        builder.AddComponentParameter(4, nameof(FaSidebarShell.UserDisplayName), UserDisplayName);
        builder.AddComponentParameter(5, nameof(FaSidebarShell.UserImageUrl), UserImageUrl);
        builder.AddComponentParameter(6, nameof(FaSidebarShell.OnLogin), OnLogin);
        builder.AddComponentParameter(7, nameof(FaSidebarShell.OnLogout), OnLogout);
        builder.AddComponentParameter(8, nameof(FaSidebarShell.Sidebar), Sidebar);
        builder.AddComponentParameter(9, nameof(FaSidebarShell.FooterContent), FooterContent);
        builder.AddComponentParameter(10, nameof(FaSidebarShell.HeaderPosition), HeaderPosition);
        builder.AddComponentParameter(11, nameof(FaSidebarShell.FooterPosition), FooterPosition);
        builder.AddComponentParameter(12, nameof(FaSidebarShell.SidebarPosition), SidebarPosition);
        builder.AddComponentParameter(13, nameof(FaSidebarShell.SidebarCollapsible), SidebarCollapsible);
        builder.AddComponentParameter(14, nameof(FaSidebarShell.ContainScroll), ContainScroll);
        builder.AddComponentParameter(15, nameof(FaSidebarShell.ChildContent), (RenderFragment)RenderBody);
        builder.CloseComponent();
    }

    private void RenderBody(RenderTreeBuilder builder)
    {
        var hasHeaderRow = !string.IsNullOrEmpty(Title) || Actions is not null;

        if (hasHeaderRow)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "fa-template-page-header");

            if (!string.IsNullOrEmpty(Title))
            {
                builder.OpenElement(2, "h1");
                builder.AddContent(3, Title);
                builder.CloseElement();
            }

            if (Actions is not null)
            {
                builder.OpenElement(4, "div");
                builder.AddAttribute(5, "class", "fa-template-page-actions");
                builder.AddContent(6, Actions);
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.AddContent(7, ChildContent);
    }
}
