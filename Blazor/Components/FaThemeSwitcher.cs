using FactoryAspects.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// Three explicit theme buttons — Light / Dark / Colorblind-safe. Deliberately no
/// Blazor state or IJSRuntime here: <c>onclick</c> (lowercase, not <c>@onclick</c>) is a
/// raw HTML attribute Blazor passes straight through, so clicking calls the global
/// <c>window.faSetTheme(...)</c> from js/theme.js directly, client-side only. Which
/// button looks "active" is also handled by that script (see markActive there), not by
/// anything this component tracks — there's nothing here for a re-render to get out of
/// sync with.
/// </summary>
public sealed class FaThemeSwitcher : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-theme-switcher");
        builder.AddAttribute(2, "role", "group");
        builder.AddAttribute(3, "aria-label", "Theme");

        RenderThemeButton(builder, 4, "light", "Light theme", FaIconName.Sun);
        RenderThemeButton(builder, 20, "dark", "Dark theme", FaIconName.Moon);
        RenderThemeButton(builder, 40, "colorblind", "Colorblind-safe theme", FaIconName.Eye);

        builder.CloseElement();
    }

    private static void RenderThemeButton(RenderTreeBuilder builder, int sequence, string theme, string title, FaIconName icon)
    {
        builder.OpenElement(sequence, "button");
        builder.AddAttribute(sequence + 1, "type", "button");
        builder.AddAttribute(sequence + 2, "class", "fa-theme-btn");
        builder.AddAttribute(sequence + 3, "data-theme-btn", theme);
        builder.AddAttribute(sequence + 4, "title", title);
        builder.AddAttribute(sequence + 5, "aria-pressed", "false");
        builder.AddAttribute(sequence + 6, "onclick", $"faSetTheme('{theme}')");

        builder.OpenComponent<FaIcon>(sequence + 7);
        builder.AddComponentParameter(sequence + 8, nameof(FaIcon.Name), icon);
        builder.AddComponentParameter(sequence + 9, nameof(FaIcon.Color), FaIconColor.White);
        builder.AddComponentParameter(sequence + 10, nameof(FaIcon.Size), 16);
        builder.AddComponentParameter(sequence + 11, nameof(FaIcon.Title), title);
        builder.CloseComponent();

        builder.CloseElement();
    }
}
