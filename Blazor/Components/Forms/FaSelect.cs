using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

/// <summary>
/// Matches the sort/filter dropdowns already in the app (e.g. Ledger's sort select) —
/// pass &lt;option&gt; elements as ChildContent, same as a plain &lt;select&gt;.
/// </summary>
public sealed class FaSelect<TValue> : InputBase<TValue>
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    /// <summary>
    /// Same boxed look as normal, just muted and non-interactive. &lt;select&gt; has no
    /// native "readonly" (unlike a text input, it'd still be focusable/openable), so
    /// this renders as <c>disabled</c> instead — the closest native equivalent.
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    private readonly string _id = $"fa-select-{Guid.NewGuid():N}";

    private void OnChange(ChangeEventArgs e) => CurrentValueAsString = e.Value?.ToString();

    protected override bool TryParseValueFromString(string? value, out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        var success = BindConverter.TryConvertTo<TValue>(value, System.Globalization.CultureInfo.CurrentCulture, out var parsed);
        result = parsed!;
        validationErrorMessage = success ? null : $"'{value}' is not a valid value.";
        return success;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", $"fa-field {ContainerCssClass}");

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(2, "label");
            builder.AddAttribute(3, "class", "fa-label");
            builder.AddAttribute(4, "for", _id);
            builder.AddContent(5, Label);
            builder.CloseElement();
        }

        builder.OpenElement(6, "select");
        builder.AddAttribute(7, "id", _id);
        builder.AddAttribute(8, "class", "fa-select");
        builder.AddAttribute(9, "value", CurrentValueAsString);
        builder.AddAttribute(10, "disabled", ReadOnly);
        builder.AddMultipleAttributes(11, AdditionalAttributes);
        builder.AddAttribute(12, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnChange));
        builder.AddContent(12, ChildContent);
        builder.CloseElement();

        builder.CloseElement();
    }
}
