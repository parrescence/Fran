using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Components;

/// <summary>
/// A single dropdown covering all twenty-eight <c>data-fa-palette</c> values (see
/// <c>theme.css</c>'s "Theme variants" section), plus whatever consumer-supplied
/// <see cref="FaPalette"/>s are passed via <see cref="CustomPalettes"/>. A `&lt;select&gt;`,
/// not a button row like <see cref="FaThemeSwitcher"/> — this many options don't fit a pill
/// row the way three modes do. Deliberately no Blazor state or IJSRuntime here, same
/// reasoning as <see cref="FaThemeSwitcher"/>: <c>onchange</c> (lowercase, not
/// <c>@onchange</c>) is a raw HTML attribute Blazor passes straight through, so picking an
/// option calls the global <c>window.faSetPalette(...)</c> from js/theme.js directly,
/// client-side only. Which option shows as selected on page load is handled by that script
/// (see <c>syncPaletteSelects</c> there), not by anything this component tracks — there's
/// nothing here for a re-render to get out of sync with.
///
/// The twenty-eight built-ins are precompiled into <c>fa-styles.css</c> and selected purely
/// by their slug (<c>data-fa-palette="..."</c>) — nothing about their color values ever
/// needs to reach the browser via C#/JSON. A <see cref="FaPalette"/> from
/// <see cref="CustomPalettes"/> has no compiled CSS to select, so its own &lt;option&gt;
/// instead carries its colors as <c>data-colors</c>/<c>data-dark-overrides</c> JSON
/// attributes (see <see cref="FaPalette"/>'s own doc comment) that <c>theme.js</c> reads at
/// selection time and applies as inline <c>--fa-*</c> custom properties on
/// <c>&lt;html&gt;</c> instead of an attribute swap.
/// </summary>
public sealed class FaPaletteSwitcher : ComponentBase
{
    private static readonly JsonSerializerOptions ColorsJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

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
        ("sahara-desert", "Sahara Desert"),
        ("brazil-rainforest", "Brazil — Rainforest"),
        ("brazil-favela", "Brazil — Favela"),
        ("portugal-tiles", "Portugal — Tiles"),
        ("spain-bullfighting", "Spain — Bullfighting"),
        ("mexico-day-of-the-dead", "Mexico — Day of the Dead"),
        ("london-life", "London Life"),
        ("new-york-nightlife", "New York Nightlife"),
        ("india-henna", "India — Henna"),
        ("ruckus", "Ruckus"),
        ("dinner", "Dinner"),
        ("hang-in", "Hang'In"),
        ("floating", "Floating"),
        ("fit", "Fit"),
    ];

    /// <summary>
    /// Consumer-supplied palettes appended after the twenty-eight built-ins, each rendered
    /// as its own &lt;option value="custom:{Value}"&gt; carrying its colors as JSON data
    /// attributes — see this class's own doc comment and <see cref="FaPalette"/>.
    /// </summary>
    [Parameter]
    public IReadOnlyList<FaPalette>? CustomPalettes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "select");
        builder.AddAttribute(1, "class", "fa-palette-switcher");
        builder.AddAttribute(2, "data-palette-select", true);
        builder.AddAttribute(3, "aria-label", "Color palette");
        builder.AddAttribute(4, "onchange", "faSetPalette(this.value, this)");

        var sequence = 5;
        foreach (var (value, label) in Palettes)
        {
            builder.OpenElement(sequence++, "option");
            builder.AddAttribute(sequence++, "value", value);
            builder.AddContent(sequence++, label);
            builder.CloseElement();
        }

        if (CustomPalettes is not null)
        {
            foreach (var palette in CustomPalettes)
            {
                builder.OpenElement(sequence++, "option");
                builder.AddAttribute(sequence++, "value", $"custom:{palette.Value}");
                builder.AddAttribute(sequence++, "data-colors", JsonSerializer.Serialize(palette.Colors, ColorsJsonOptions));
                if (palette.DarkOverrides is not null)
                {
                    builder.AddAttribute(sequence++, "data-dark-overrides", JsonSerializer.Serialize(palette.DarkOverrides, ColorsJsonOptions));
                }
                builder.AddContent(sequence++, palette.Label);
                builder.CloseElement();
            }
        }

        builder.CloseElement();
    }
}
