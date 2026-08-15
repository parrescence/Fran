using FactoryAspects.Icons;
using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Layout;

/// <summary>
/// Left sidebar shell — same background color as the header by design. Nav links go
/// through ChildContent; the app's own NavMenu supplies them (e.g. FinanceApp.Web's
/// NavMenu.razor, an example consumer, for real links + auth-aware admin check).
///
/// The collapse toggle is deliberately plain JS (onclick -> window.faToggleSidebar,
/// see js/sidebar.js), same reasoning as FaThemeSwitcher: collapsed/expanded is pure
/// client-side UI state stamped on &lt;html&gt;, with no Blazor state to keep in sync.
/// </summary>
public sealed class FaSidebar : ComponentBase
{
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether the sidebar scrolls away with the page (default) or stays pinned
    /// full-height. Pinning means the sidebar's own content scrolls independently
    /// (via <c>overflow-y: auto</c> on an inner wrapper, not the sidebar element
    /// itself — see <see cref="BuildRenderTree"/>) once it's taller than the
    /// viewport, since the sidebar itself no longer scrolls away with the rest of
    /// the page.
    /// </summary>
    [Parameter] public FaNavPosition Position { get; set; } = FaNavPosition.Standard;

    /// <summary>
    /// Whether the sidebar can be manually collapsed to an icon-only rail via its
    /// own toggle button. Defaults to <c>false</c> — no toggle renders, and the
    /// sidebar always shows expanded (small screens still auto-adapt to a compact
    /// row regardless of this, via <c>_responsive.scss</c>'s own breakpoint — that's
    /// unrelated to this parameter). Collapsing to an icon-only rail only reads
    /// correctly for content that has an icon of its own to fall back to; set
    /// <c>true</c> only once <see cref="ChildContent"/>'s links are all set up that
    /// way (a plain-text link with no icon just vanishes while collapsed rather
    /// than wrapping into an illegible sliver — see <c>_layout.scss</c>).
    /// </summary>
    [Parameter] public bool Collapsible { get; set; }

    private string? PositionClass => Position switch
    {
        FaNavPosition.Sticky => "fa-sidebar-sticky",
        FaNavPosition.Floating => "fa-sidebar-floating",
        _ => null
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "aside");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-sidebar", Collapsible ? "fa-sidebar-collapsible" : null, PositionClass, CssClass));

        if (Collapsible)
        {
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
        }

        // Content lives in its own scrolling wrapper rather than making .fa-sidebar
        // itself overflow: auto — the toggle button above (and the collapsed-state
        // tooltip fly-outs inside ChildContent) are positioned outside/beyond this
        // element's own box and would get clipped by an overflow:auto on their
        // ancestor. Keeping that overflow one level down, on a sibling of the
        // toggle, lets .fa-sidebar stay overflow: visible so both still spill out
        // over the pinned sidebar's edge the way they're meant to.
        builder.OpenElement(14, "div");
        builder.AddAttribute(15, "class", "fa-sidebar-scroll");
        builder.AddContent(16, ChildContent);
        builder.CloseElement();

        builder.CloseElement();
    }
}
