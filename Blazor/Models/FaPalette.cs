namespace Fran.Components;

/// <summary>
/// A consumer-defined color palette to add to <see cref="FaPaletteSwitcher"/> alongside
/// its twenty-eight built-ins (see <c>Blazor/CLAUDE.md</c>'s "Themes" section for those —
/// they're precompiled into <c>fa-styles.css</c> and picked via the <c>data-fa-palette</c>
/// attribute). A custom palette has no compiled CSS to select: <see cref="Value"/> is only
/// ever used prefixed as <c>"custom:{Value}"</c> inside the &lt;option&gt;
/// <see cref="FaPaletteSwitcher"/> renders for it, and its <see cref="Colors"/>/
/// <see cref="DarkOverrides"/> travel with that option as JSON data attributes
/// <c>theme.js</c> reads at selection time and applies as inline <c>--fa-*</c> custom
/// properties on <c>&lt;html&gt;</c> — never added to <c>fa-styles.css</c> itself. Pass one
/// or more via <see cref="FaPaletteSwitcher"/>'s own <c>CustomPalettes</c> parameter:
/// <code>
/// &lt;FaPaletteSwitcher CustomPalettes="myPalettes" /&gt;
/// </code>
/// See <c>docs/palette-switcher.md</c> for the full walkthrough.
/// </summary>
/// <param name="Value">
/// A URL/attribute-safe slug, unique among your own custom palettes (built-ins already
/// use their own slugs, e.g. <c>"southwest-summer"</c> — reusing one of those as a custom
/// <see cref="Value"/> is fine; they render as separate, independent options).
/// </param>
/// <param name="Label">Display text shown in the dropdown.</param>
/// <param name="Colors">Full light-mode color set — every token is required.</param>
/// <param name="DarkOverrides">
/// Optional dark-mode overrides, applied on top of <see cref="Colors"/> only when dark
/// mode (<c>data-theme="dark"</c>, or the OS preference when that attribute is absent) is
/// active. Leave <c>null</c> for a palette that doesn't change between modes; otherwise set
/// only the tokens that actually should change — the same sparse-override shape every
/// built-in palette's own dark block uses.
/// </param>
public sealed record FaPalette(
    string Value,
    string Label,
    FaPaletteColors Colors,
    FaPaletteDarkOverrides? DarkOverrides = null);

/// <summary>
/// The full light-mode token set a <see cref="FaPalette"/> must supply — the same
/// twenty-one <c>--fa-*</c> custom properties every built-in palette's own light-mode
/// block redefines in full (see <c>wwwroot/css/scss/_palettes.scss</c>). Any valid CSS
/// color string (hex, <c>rgb()</c>, a named color, ...).
/// </summary>
public sealed record FaPaletteColors(
    string Primary,
    string PrimaryDark,
    string PrimaryLight,
    string Footer,
    string Glow,
    string Gold,
    string Accent,
    string AccentDark,
    string Ember,
    string Wine,
    string Fir,
    string Cream,
    string Surface,
    string Text,
    string TextMuted,
    string TextOnPrimary,
    string Border,
    string BorderFocus,
    string AlertDangerBg,
    string AlertSuccessBg,
    string AlertInfoBg);

/// <summary>
/// Sparse dark-mode overrides for a <see cref="FaPalette"/> — mirrors the smaller token
/// set every built-in palette's own <c>[data-theme="dark"]</c> block redefines, not the
/// full <see cref="FaPaletteColors"/> set. Leave any property <c>null</c> to keep that
/// token at its <see cref="FaPalette.Colors"/> light-mode value in dark mode too.
/// </summary>
public sealed record FaPaletteDarkOverrides(
    string? Cream = null,
    string? Surface = null,
    string? Text = null,
    string? TextMuted = null,
    string? Border = null,
    string? BorderFocus = null,
    string? Footer = null,
    string? PrimaryDark = null,
    string? AccentDark = null,
    string? AlertDangerBg = null,
    string? AlertSuccessBg = null,
    string? AlertInfoBg = null);
