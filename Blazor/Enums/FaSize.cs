namespace Fran.Components;

/// <summary>
/// Shared size scale for <c>Size</c> parameters across the library (buttons, form
/// inputs, cards, alerts, badges, chips — see each component's own doc page for
/// whether it participates). <see cref="Medium"/> is the default on every one of
/// them and renders with no extra CSS class at all — it's today's un-sized look, so
/// adding <c>Size</c> to a component is never a visual change for a consumer who
/// doesn't set it. Each component tunes its own padding/font-size per step (see
/// <c>Blazor/CLAUDE.md</c>'s "Themes" section on why spacing is hand-tuned per
/// component rather than a shared token) — the scale itself is what's shared, not
/// the literal values behind it.
/// </summary>
public enum FaSize
{
    XSmall,
    Small,
    Medium,
    Large,
    XLarge
}
