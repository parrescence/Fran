using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// Three explicit buttons — Standard / Minimal / Maximal — for the global input-style
/// axis (see <c>_inputs.scss</c>'s own comment): a third, independent axis alongside
/// palette (<see cref="FaPaletteSwitcher"/>) and mode (<see cref="FaThemeSwitcher"/>),
/// all stamped as data attributes on <c>&lt;html&gt;</c>. It retunes the boxed
/// native-input-like controls (<c>FaInput</c>/<c>FaSelect</c>/<c>FaTextarea</c>/
/// <c>FaCurrency</c>) app-wide — thinner border, no radius, no shadow for Minimal;
/// heavier border and a larger radius for Maximal — with nothing to set per
/// component instance, unlike <c>FaInput</c>/etc.'s own per-instance <c>Size</c>.
/// Same deliberately-no-Blazor-state pattern as <see cref="FaThemeSwitcher"/>:
/// <c>onclick</c> (lowercase) calls the global <c>window.faSetInputStyle(...)</c>
/// (js/theme.js) directly, client-side only — which button looks "active" is handled
/// by that script too, not tracked here.
/// </summary>
public sealed class FaInputStyleSwitcher : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "fa-input-style-switcher");
        builder.AddAttribute(2, "role", "group");
        builder.AddAttribute(3, "aria-label", "Input style");

        RenderButton(builder, 4, "standard", "Standard");
        RenderButton(builder, 10, "minimal", "Minimal");
        RenderButton(builder, 16, "maximal", "Maximal");

        builder.CloseElement();
    }

    private static void RenderButton(RenderTreeBuilder builder, int sequence, string inputStyle, string label)
    {
        builder.OpenElement(sequence, "button");
        builder.AddAttribute(sequence + 1, "type", "button");
        builder.AddAttribute(sequence + 2, "class", "fa-input-style-btn");
        builder.AddAttribute(sequence + 3, "data-input-style-btn", inputStyle);
        builder.AddAttribute(sequence + 4, "aria-pressed", "false");
        builder.AddAttribute(sequence + 5, "onclick", $"faSetInputStyle('{inputStyle}')");
        builder.AddContent(sequence + 6, label);
        builder.CloseElement();
    }
}
