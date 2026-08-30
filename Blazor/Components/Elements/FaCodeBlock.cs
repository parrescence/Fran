using FaFa.Icons;
using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Components;

/// <summary>
/// A read-only source-code panel: a language-labeled header, an optional copy
/// button, and lightly syntax-highlighted content (see
/// <c>Rendering/CodeHighlighter.cs</c> — regex-driven token classification, not a
/// full parser or a third-party highlighting library). Always read-only — this
/// isn't an editable field, so it doesn't derive from <c>InputBase&lt;TValue&gt;</c>
/// or take a bound <c>Value</c> the way <see cref="FaTextarea"/> does.
/// </summary>
/// <remarks>
/// The copy button's actual clipboard write (<c>wwwroot/js/codeblock.js</c>) is one
/// of the few things in this library that genuinely can't be expressed in Blazor's
/// own event model — <c>navigator.clipboard.writeText</c> has no Blazor-native
/// equivalent — so it's a plain vanilla-JS IIFE invoked via a raw <c>onclick</c>
/// HTML attribute, the same pattern <see cref="FaThemeSwitcher"/>/
/// <see cref="FaPaletteSwitcher"/> use for their own JS calls, not
/// <c>IJSRuntime.InvokeVoidAsync</c>.
/// </remarks>
public sealed class FaCodeBlock : ComponentBase
{
    [Parameter, EditorRequired] public string Code { get; set; } = "";
    [Parameter] public FaCodeLanguage Language { get; set; } = FaCodeLanguage.PlainText;

    /// <summary>
    /// Shows/hides the copy-to-clipboard button in the header. The language label
    /// stays visible either way — this only toggles whether the snippet is
    /// copyable, not whether it's identified.
    /// </summary>
    [Parameter] public bool Copyable { get; set; } = true;

    [Parameter] public string? CssClass { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // Stable id, generated once — pairs the copy button's data-copy-target with the
    // <code> element codeblock.js reads innerText from. Never regenerate this inside
    // BuildRenderTree; see Blazor/CLAUDE.md's "stable ids are field initializers" rule.
    private readonly string _id = $"fa-codeblock-{Guid.NewGuid():N}";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var (slug, label) = CodeHighlighter.Describe(Language);
        var highlighted = CodeHighlighter.Highlight(Code, Language);

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-codeblock", CssClass));
        builder.AddMultipleAttributes(2, AdditionalAttributes);

        builder.OpenElement(3, "div");
        builder.AddAttribute(4, "class", "fa-codeblock-header");

        builder.OpenElement(5, "span");
        builder.AddAttribute(6, "class", "fa-codeblock-lang");
        builder.AddContent(7, label);
        builder.CloseElement(); // span.fa-codeblock-lang

        if (Copyable)
        {
            builder.OpenElement(8, "button");
            builder.AddAttribute(9, "type", "button");
            builder.AddAttribute(10, "class", "fa-codeblock-copy");
            builder.AddAttribute(11, "data-copy-target", _id);
            builder.AddAttribute(12, "aria-label", "Copy code");
            builder.AddAttribute(13, "onclick", "faCopyCodeBlock(this)");

            builder.OpenComponent<FaIcon>(14);
            builder.AddComponentParameter(15, nameof(FaIcon.Name), FaIconName.Copy);
            builder.AddComponentParameter(16, nameof(FaIcon.Size), 14);
            builder.AddComponentParameter(17, nameof(FaIcon.CssClass), "fa-codeblock-copy-icon");
            builder.CloseComponent();

            builder.OpenElement(18, "span");
            builder.AddAttribute(19, "class", "fa-codeblock-copy-label");
            builder.AddContent(20, "Copy");
            builder.CloseElement(); // span.fa-codeblock-copy-label

            builder.CloseElement(); // button.fa-codeblock-copy
        }

        builder.CloseElement(); // div.fa-codeblock-header

        builder.OpenElement(21, "pre");
        builder.AddAttribute(22, "class", "fa-codeblock-pre");

        builder.OpenElement(23, "code");
        builder.AddAttribute(24, "id", _id);
        builder.AddAttribute(25, "class", $"fa-codeblock-code language-{slug}");
        builder.AddContent(26, new MarkupString(highlighted));
        builder.CloseElement(); // code

        builder.CloseElement(); // pre
        builder.CloseElement(); // div.fa-codeblock
    }
}
