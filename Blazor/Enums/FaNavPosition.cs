namespace Fran.Layout;

/// <summary>
/// How a layout bar (<see cref="FaHeader"/>, <see cref="FaFooter"/>, <see
/// cref="FaSidebar"/>) sits relative to the page as it scrolls. Shared enum, one per
/// component's own <c>Position</c> parameter — each maps it to its own prefixed CSS
/// class (<c>.fa-header-*</c>/<c>.fa-footer-*</c>/<c>.fa-sidebar-*</c>) rather than a
/// single shared class, since the underlying mechanics differ per bar (header pins to
/// the top, footer to the bottom, sidebar additionally needs its own internal scroll
/// once pinned full-height — see <c>_layout.scss</c>).
/// </summary>
public enum FaNavPosition
{
    /// <summary>Normal document flow — scrolls away with the page. The default.</summary>
    Standard,
    /// <summary>Pinned to its edge of the viewport once scrolled to (<c>position: sticky</c>).</summary>
    Sticky,
    /// <summary>Same pinning as <see cref="Sticky"/>, inset with margin/rounded corners/shadow so it reads as a detached bar over the content.</summary>
    Floating
}
