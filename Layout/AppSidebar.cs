using FactoryAspects.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Layout;

/// <summary>
/// Left sidebar shell — same background color as the header by design. Nav links go
/// through ChildContent; the app's own NavMenu supplies them (e.g. FinanceApp.Web's
/// NavMenu.razor, an example consumer, for real links + auth-aware admin check).
///
/// The collapse toggle is deliberately plain JS (onclick -> window.faToggleSidebar,
/// see js/sidebar.js), same reasoning as ThemeSwitcher: collapsed/expanded is pure
/// client-side UI state stamped on &lt;html&gt;, with no Blazor state to keep in sync.
/// </summary>
public sealed class AppSidebar : ComponentBase
{
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "aside");
        builder.AddAttribute(1, "class", $"fa-sidebar {CssClass}");

        builder.OpenElement(2, "button");
        builder.AddAttribute(3, "type", "button");
        builder.AddAttribute(4, "class", "fa-sidebar-toggle");
        builder.AddAttribute(5, "data-sidebar-toggle", true);
        builder.AddAttribute(6, "title", "Collapse sidebar");
        builder.AddAttribute(7, "aria-label", "Collapse sidebar");
        builder.AddAttribute(8, "aria-expanded", "true");
        builder.AddAttribute(9, "onclick", "faToggleSidebar()");

        builder.OpenComponent<FaIcon>(10);
        builder.AddComponentParameter(11, nameof(FaIcon.Name), FaIconName.ChevronLeft);
        builder.AddComponentParameter(12, nameof(FaIcon.Color), FaIconColor.White);
        builder.AddComponentParameter(13, nameof(FaIcon.Size), 14);
        builder.CloseComponent();

        builder.CloseElement();

        builder.AddContent(14, ChildContent);

        builder.CloseElement();
    }
}
