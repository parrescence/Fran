using Fran.Icons;
using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// Stacked collapsible sections — FAQ list, settings groups. AllowMultipleOpen
/// controls whether opening a section closes the others (the classic accordion
/// behavior, the default) or each section tracks its own open state independently.
/// Open/closed state lives entirely in this component (a plain HashSet&lt;int&gt; of
/// open indexes) — nothing for a caller to bind unless InitialOpenIndex is set.
/// Every panel is always rendered (never conditionally skipped for a closed
/// section) inside a CSS grid-rows wrapper that animates 0fr/1fr on open/close —
/// the no-JS way to get a smooth height transition without measuring content.
/// </summary>
public sealed class FaAccordion : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<(string Title, RenderFragment Content)> Items { get; set; } = Array.Empty<(string, RenderFragment)>();
    [Parameter] public bool AllowMultipleOpen { get; set; }
    [Parameter] public int? InitialOpenIndex { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private readonly HashSet<int> _openIndexes = new();
    private bool _initialized;

    protected override void OnParametersSet()
    {
        if (_initialized)
            return;

        _initialized = true;
        if (InitialOpenIndex is { } index && index >= 0 && index < Items.Count)
        {
            _openIndexes.Add(index);
        }
    }

    private void Toggle(int index)
    {
        if (_openIndexes.Contains(index))
        {
            _openIndexes.Remove(index);
            return;
        }

        if (!AllowMultipleOpen)
        {
            _openIndexes.Clear();
        }

        _openIndexes.Add(index);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-accordion", CssClass));

        for (var i = 0; i < Items.Count; i++)
        {
            var index = i;
            var (title, content) = Items[index];
            var isOpen = _openIndexes.Contains(index);
            var panelId = $"fa-accordion-panel-{index}";

            builder.OpenElement(2, "div");
            builder.SetKey(index);
            builder.AddAttribute(3, "class", CssClassNames.Combine("fa-accordion-item", isOpen ? "fa-accordion-item-open" : null));

            builder.OpenElement(4, "button");
            builder.AddAttribute(5, "type", "button");
            builder.AddAttribute(6, "class", "fa-accordion-header");
            builder.AddAttribute(7, "aria-expanded", isOpen ? "true" : "false");
            builder.AddAttribute(8, "aria-controls", panelId);
            builder.AddAttribute(9, "onclick", EventCallback.Factory.Create(this, () => Toggle(index)));

            builder.OpenElement(10, "span");
            builder.AddContent(11, title);
            builder.CloseElement();

            builder.OpenElement(12, "span");
            builder.AddAttribute(13, "class", "fa-accordion-chevron");
            builder.OpenComponent<FaIcon>(14);
            builder.AddComponentParameter(15, nameof(FaIcon.Name), FaIconName.ChevronDown);
            builder.AddComponentParameter(16, nameof(FaIcon.Color), FaIconColor.Black);
            builder.AddComponentParameter(17, nameof(FaIcon.Size), 16);
            builder.CloseComponent();
            builder.CloseElement();

            builder.CloseElement();

            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "class", "fa-accordion-panel-wrapper");
            builder.OpenElement(22, "div");
            builder.AddAttribute(23, "id", panelId);
            builder.AddAttribute(24, "class", "fa-accordion-panel");
            builder.AddAttribute(25, "aria-hidden", isOpen ? null : "true");
            builder.AddContent(26, content);
            builder.CloseElement();
            builder.CloseElement();

            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
