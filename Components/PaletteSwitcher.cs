using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// A single dropdown covering all fourteen <c>data-fa-palette</c> values (see
/// <c>theme.css</c>'s "Theme variants" section). A `&lt;select&gt;`, not a button row like
/// <see cref="ThemeSwitcher"/> — fourteen options don't fit a pill row the way three modes
/// do. Deliberately no Blazor state or IJSRuntime here, same reasoning as
/// <see cref="ThemeSwitcher"/>: <c>onchange</c> (lowercase, not <c>@onchange</c>) is a raw
/// HTML attribute Blazor passes straight through, so picking an option calls the global
/// <c>window.faSetPalette(...)</c> from js/theme.js directly, client-side only. Which
/// option shows as selected on page load is handled by that script (see
/// <c>syncPaletteSelects</c> there), not by anything this component tracks — there's
/// nothing here for a re-render to get out of sync with.
/// </summary>
public sealed class PaletteSwitcher : ComponentBase
{
    private static readonly (string Value, string Label)[] Palettes =
    [
        ("", "Northwest Fall"),
        ("southwest-summer", "Southwest Summer"),
        ("northeast-spring", "Northeast Spring"),
        ("midwest-winter", "Midwest Winter"),
        ("southeast-beach", "Southeast Beach"),
        ("greece-aegean", "Greece — Aegean"),
        ("spain-flamenco", "Spain — Flamenco"),
        ("ireland-emerald", "Ireland — Emerald"),
        ("jamaica-blue-mountain", "Jamaica — Blue Mountain"),
        ("japan-indigo", "Japan — Indigo"),
        ("korea-celadon", "Korea — Celadon"),
        ("china-cinnabar", "China — Cinnabar"),
        ("india-peacock", "India — Peacock"),
        ("cameroon-rainforest", "Cameroon — Rainforest"),
    ];

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "select");
        builder.AddAttribute(1, "class", "fa-palette-switcher");
        builder.AddAttribute(2, "data-palette-select", true);
        builder.AddAttribute(3, "aria-label", "Color palette");
        builder.AddAttribute(4, "onchange", "faSetPalette(this.value)");

        var sequence = 5;
        foreach (var (value, label) in Palettes)
        {
            builder.OpenElement(sequence++, "option");
            builder.AddAttribute(sequence++, "value", value);
            builder.AddContent(sequence++, label);
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
