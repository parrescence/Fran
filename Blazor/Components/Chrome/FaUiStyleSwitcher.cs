using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Components;

/// <summary>
/// Three explicit buttons — Flow / Terse / Typewriter — for the global UI-style
/// axis (see <c>_palettes.scss</c>'s own comment): a fourth, independent axis
/// alongside palette (<see cref="FaPaletteSwitcher"/>), mode
/// (<see cref="FaThemeSwitcher"/>), and input style
/// (<see cref="FaInputStyleSwitcher"/>), all stamped as data attributes on
/// <c>&lt;html&gt;</c>. Unlike input style, which only retunes the boxed native-
/// input-like controls, this one retunes the shared shape/type/motion tokens
/// (radius, font, border-glow shadow, transition speed) every component already
/// draws from — Flow is today's rounded, softly animated look; Terse is a flatter,
/// editorial one with smaller radii, a plain system font, no glow shadow, and no
/// transitions; Typewriter is a paper-like one with a monospaced serif font,
/// moderate radii, and a soft neutral "resting" shadow standing in for the glow.
/// Same deliberately-no-Blazor-state pattern as
/// <see cref="FaThemeSwitcher"/>/<see cref="FaInputStyleSwitcher"/>: <c>onclick</c>
/// (lowercase) calls the global <c>window.faSetUiStyle(...)</c> (js/theme.js)
/// directly, client-side only — which button looks "active" is handled by that
/// script too, not tracked here.
/// </summary>
public sealed class FaUiStyleSwitcher : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-ui-style-switcher");
        builder.AddAttribute(2, "role", "group");
        builder.AddAttribute(3, "aria-label", "UI style");

        RenderButton(builder, 4, "flow", "Flow");
        RenderButton(builder, 10, "terse", "Terse");
        RenderButton(builder, 16, "typewriter", "Typewriter");

        builder.CloseElement();
    }

    private static void RenderButton(RenderTreeBuilder builder, int sequence, string uiStyle, string label)
    {
        builder.OpenElement(sequence, "button");
        builder.AddAttribute(sequence + 1, "type", "button");
        builder.AddAttribute(sequence + 2, "class", "fa-ui-style-btn");
        builder.AddAttribute(sequence + 3, "data-ui-style-btn", uiStyle);
        builder.AddAttribute(sequence + 4, "aria-pressed", "false");
        builder.AddAttribute(sequence + 5, "onclick", $"faSetUiStyle('{uiStyle}')");
        builder.AddContent(sequence + 6, label);
        builder.CloseElement();
    }
}
