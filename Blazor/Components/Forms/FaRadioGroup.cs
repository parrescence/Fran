using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// A group of radio buttons — pass 1+ (Title, Value) options, same shape as
/// <see cref="FaToggle{TValue}"/>'s Options. There's no separate standalone
/// <c>FaRadio</c> component: a lone radio button needs a stable, shared <c>name</c>
/// to belong to a group, and generating that name here (once, as a field
/// initializer) rather than asking callers to coordinate it themselves is exactly
/// what the older library this was ported from got wrong — its radio group name was
/// a fresh <c>Guid.NewGuid()</c> on every render, which silently broke the group.
/// </summary>
public sealed class FaRadioGroup<TValue> : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<(string Title, TValue Value)> Options { get; set; } = Array.Empty<(string, TValue)>();
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public bool Disabled { get; set; }
    /// <summary>
    /// Flattens the radio buttons to the selected option's title as plain text with
    /// a bottom border — the shared "other fa styles" readonly look (see
    /// FaInput/FaSelect/etc. for the boxed-muted style used by native inputs).
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    private readonly string _groupName = $"fa-radio-group-{Guid.NewGuid():N}";

    private async Task SelectAsync(TValue? value)
    {
        if (Disabled)
        {
            return;
        }

        if (!EqualityComparer<TValue?>.Default.Equals(Value, value))
        {
            Value = value;
            await ValueChanged.InvokeAsync(value);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-field", CssClass));

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(2, "span");
            builder.AddAttribute(3, "class", "fa-label");
            builder.AddContent(4, Label);
            builder.CloseElement();
        }

        if (ReadOnly)
        {
            var activeTitle = Options.FirstOrDefault(o => EqualityComparer<TValue?>.Default.Equals(o.Value, Value)).Title;
            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "fa-readonly-flat");
            builder.AddContent(7, activeTitle);
            builder.CloseElement();
            builder.CloseElement();
            return;
        }

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "class", "fa-radio-group");
        builder.AddAttribute(7, "role", "radiogroup");
        builder.AddAttribute(8, "aria-label", Label);

        var seq = 9;
        foreach (var (title, value) in Options)
        {
            var isChecked = EqualityComparer<TValue?>.Default.Equals(Value, value);
            var capturedValue = value;

            builder.OpenElement(seq++, "label");
            builder.SetKey(value);
            builder.AddAttribute(seq++, "class", "fa-radio");

            builder.OpenElement(seq++, "input");
            builder.AddAttribute(seq++, "type", "radio");
            builder.AddAttribute(seq++, "class", "fa-radio-input");
            builder.AddAttribute(seq++, "name", _groupName);
            builder.AddAttribute(seq++, "checked", isChecked);
            builder.AddAttribute(seq++, "disabled", Disabled);
            builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create(this, () => SelectAsync(capturedValue)));
            builder.CloseElement();

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "fa-radio-label");
            builder.AddContent(seq++, title);
            builder.CloseElement();

            builder.CloseElement();
        }

        builder.CloseElement();
        builder.CloseElement();
    }
}
