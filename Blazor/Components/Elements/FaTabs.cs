using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Fran.Components;

/// <summary>
/// Tab strip — pass 2+ (Title, Value) options, same tuple shape as FaToggle, but a
/// proper role="tablist"/role="tab" strip with an underline active-indicator
/// instead of a segmented pill row. Pick this over FaToggle when the options are
/// actually different views/sections of content, not just a value picker. FaTabs
/// only renders the strip — which panel is showing is your own Razor's @if on
/// whichever value you bind, the same "no hidden content ownership" shape
/// FaDropdown/FaSearchSelect already use for their own selection state.
/// </summary>
public sealed class FaTabs<TValue> : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<(string Title, TValue Value)> Options { get; set; } = Array.Empty<(string, TValue)>();
    [Parameter] public TValue Value { get; set; } = default!;
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private ElementReference[] _tabRefs = Array.Empty<ElementReference>();

    protected override void OnParametersSet()
    {
        if (Options is null || Options.Count < 2)
            throw new ArgumentException("FaTabs requires at least two Options.", nameof(Options));

        if (_tabRefs.Length != Options.Count)
            _tabRefs = new ElementReference[Options.Count];
    }

    private async Task SelectAsync(int index)
    {
        var newValue = Options[index].Value;
        if (!EqualityComparer<TValue>.Default.Equals(Value, newValue))
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(Value);
        }
    }

    // Left/right (and Home/End) move both the selection and focus together, same as
    // FaToggle's own arrow-key handling and a native tablist's expected behavior.
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        var currentIndex = IndexOfCurrentValue();
        var fallback = currentIndex < 0 ? 0 : currentIndex;

        int? targetIndex = e.Key switch
        {
            "ArrowLeft" => Math.Max(0, fallback - 1),
            "ArrowRight" => Math.Min(Options.Count - 1, fallback + 1),
            "Home" => 0,
            "End" => Options.Count - 1,
            _ => null
        };

        if (targetIndex is int index)
        {
            await SelectAsync(index);
            await _tabRefs[index].FocusAsync();
        }
    }

    private int IndexOfCurrentValue()
    {
        for (var i = 0; i < Options.Count; i++)
        {
            if (EqualityComparer<TValue>.Default.Equals(Options[i].Value, Value))
                return i;
        }
        return -1;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-tabs", CssClass));
        builder.AddAttribute(2, "role", "tablist");
        builder.AddAttribute(3, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDown));

        for (var i = 0; i < Options.Count; i++)
        {
            var index = i;
            var option = Options[index];
            var isActive = EqualityComparer<TValue>.Default.Equals(option.Value, Value);

            builder.OpenElement(4, "button");
            builder.SetKey(index);
            builder.AddAttribute(5, "type", "button");
            builder.AddAttribute(6, "class", CssClassNames.Combine("fa-tabs-tab", isActive ? "fa-tabs-tab-active" : null));
            builder.AddAttribute(7, "role", "tab");
            builder.AddAttribute(8, "aria-selected", isActive ? "true" : "false");
            builder.AddAttribute(9, "tabindex", isActive ? "0" : "-1");
            builder.AddAttribute(10, "onclick", EventCallback.Factory.Create(this, () => SelectAsync(index)));
            builder.AddElementReferenceCapture(11, elementReference => _tabRefs[index] = elementReference);
            builder.AddContent(12, option.Title);
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
