using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

/// <summary>
/// Generic segmented toggle/switch — pass 2+ (Title, Value) options; the active one is
/// highlighted, same visual idea as FaThemeSwitcher's row of buttons (not a boolean-only
/// on/off knob). Options are plain System.ValueTuple, not a custom DTO type, so this
/// stays self-contained enough to ship standalone as a NuGet package later without
/// dragging in an app-specific model. An optional companion input can show inline only
/// while the current Value satisfies ShowInputWhen, sharing the same bordered group:
/// compact while there's no input showing, and only then does the group expand to fill
/// the field's width, with the input absorbing the extra space (see
/// .fa-toggle-group-expanded in theme.css).
/// Not InputBase-based (that requires an EditContext/EditForm, which nothing in this app
/// uses yet — see FaSelect/FaInput) — plain two-way-bindable parameters instead, same
/// pattern as FaButton/FaCard. Supports @bind-Value and @bind-InputValue.
/// </summary>
public sealed class FaToggle<TValue> : ComponentBase
{
    /// <summary>At least two (Title, Value) options — Title is the button label, Value is what gets bound/returned.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<(string Title, TValue Value)> Options { get; set; } = Array.Empty<(string, TValue)>();
    [Parameter] public TValue Value { get; set; } = default!;
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    [Parameter] public string? InputValue { get; set; }
    [Parameter] public EventCallback<string?> InputValueChanged { get; set; }

    /// <summary>HTML input type (e.g. "text", "number") for the companion input. Null renders no input at all.</summary>
    [Parameter] public string? InputType { get; set; }
    [Parameter] public FaTogglePosition InputPosition { get; set; } = FaTogglePosition.After;
    [Parameter] public string? InputPlaceholder { get; set; }
    /// <summary>Whether the companion input shows for the current Value. Ignored when InputType is null.</summary>
    [Parameter] public Func<TValue, bool>? ShowInputWhen { get; set; }

    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    /// <summary>
    /// Flattens the segmented pill row to the active option's title as plain text
    /// with a bottom border — the shared "other fa styles" readonly look (see
    /// FaInput/FaSelect/etc. for the boxed-muted style used by native inputs).
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    private ElementReference[] _buttonRefs = Array.Empty<ElementReference>();

    private bool ShowInput => InputType is not null && ShowInputWhen is not null && ShowInputWhen(Value);

    protected override void OnParametersSet()
    {
        if (Options is null || Options.Count < 2)
            throw new ArgumentException("FaToggle requires at least two Options.", nameof(Options));

        if (_buttonRefs.Length != Options.Count)
            _buttonRefs = new ElementReference[Options.Count];
    }

    private async Task SelectAsync(int index)
    {
        if (Disabled)
            return;

        var newValue = Options[index].Value;
        if (!EqualityComparer<TValue>.Default.Equals(Value, newValue))
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(Value);
        }
    }

    // Left/right arrows move both the selection and focus together, same as a native
    // radio group — not just Tab between the option buttons.
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Disabled)
            return;

        var currentIndex = IndexOfCurrentValue();
        var fallback = currentIndex < 0 ? 0 : currentIndex;

        int? targetIndex = e.Key switch
        {
            "ArrowLeft" => Math.Max(0, fallback - 1),
            "ArrowRight" => Math.Min(Options.Count - 1, fallback + 1),
            _ => null
        };

        if (targetIndex is int index)
        {
            await SelectAsync(index);
            await _buttonRefs[index].FocusAsync();
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

    private async Task OnInputChanged(ChangeEventArgs e)
    {
        InputValue = e.Value?.ToString();
        await InputValueChanged.InvokeAsync(InputValue);
    }

    private void RenderCompanionInput(RenderTreeBuilder builder, int sequence, string cssClass)
    {
        builder.OpenElement(sequence, "input");
        builder.AddAttribute(sequence + 1, "class", cssClass);
        builder.AddAttribute(sequence + 2, "type", InputType);
        builder.AddAttribute(sequence + 3, "placeholder", InputPlaceholder);
        builder.AddAttribute(sequence + 4, "value", InputValue);
        builder.AddAttribute(sequence + 5, "disabled", Disabled);
        builder.AddAttribute(sequence + 6, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInputChanged));
        builder.CloseElement();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-field", CssClass));

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(2, "label");
            builder.AddAttribute(3, "class", "fa-label");
            builder.AddContent(4, Label);
            builder.CloseElement();
        }

        if (ReadOnly)
        {
            var activeTitle = Options.FirstOrDefault(o => EqualityComparer<TValue>.Default.Equals(o.Value, Value)).Title;
            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "fa-readonly-flat");
            builder.AddContent(7, ShowInput && !string.IsNullOrEmpty(InputValue) ? $"{activeTitle} — {InputValue}" : activeTitle);
            builder.CloseElement();
            builder.CloseElement();
            return;
        }

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "class", CssClassNames.Combine("fa-toggle-group", ShowInput ? "fa-toggle-group-expanded" : null));

        if (ShowInput && InputPosition == FaTogglePosition.Before)
        {
            RenderCompanionInput(builder, 7, "fa-toggle-input fa-toggle-input-before");
        }

        builder.OpenElement(20, "div");
        builder.AddAttribute(21, "class", "fa-toggle-switch");
        builder.AddAttribute(22, "role", "group");
        builder.AddAttribute(23, "aria-label", Label);
        builder.AddAttribute(24, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDown));

        for (var i = 0; i < Options.Count; i++)
        {
            var index = i;
            var option = Options[index];
            var isActive = EqualityComparer<TValue>.Default.Equals(option.Value, Value);

            builder.OpenElement(25, "button");
            builder.SetKey(index);
            builder.AddAttribute(26, "type", "button");
            builder.AddAttribute(27, "class", CssClassNames.Combine("fa-toggle-option", isActive ? "fa-toggle-option-active" : null));
            builder.AddAttribute(28, "aria-pressed", isActive ? "true" : "false");
            builder.AddAttribute(29, "disabled", Disabled);
            builder.AddAttribute(30, "onclick", EventCallback.Factory.Create(this, () => SelectAsync(index)));
            builder.AddElementReferenceCapture(31, elementReference => _buttonRefs[index] = elementReference);
            builder.AddContent(32, option.Title);
            builder.CloseElement();
        }

        builder.CloseElement();

        if (ShowInput && InputPosition == FaTogglePosition.After)
        {
            RenderCompanionInput(builder, 40, "fa-toggle-input");
        }

        builder.CloseElement();
        builder.CloseElement();
    }
}
