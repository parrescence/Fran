using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FactoryAspects.Components;

/// <summary>
/// Generic text/number field, matching the inputs already used across the app
/// (account id, amount, description, search, etc.) — full @bind support via
/// InputBase&lt;TValue&gt;, just re-skinned with the fa- palette classes.
/// </summary>
public sealed class FaInput<TValue> : InputBase<TValue>
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string? Step { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }

    private readonly string _id = $"fa-input-{Guid.NewGuid():N}";

    private void OnInput(ChangeEventArgs e) => CurrentValueAsString = e.Value?.ToString();

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

        builder.OpenElement(6, "input");
        builder.AddAttribute(7, "id", _id);
        builder.AddAttribute(8, "class", "fa-input");
        builder.AddAttribute(9, "type", Type);
        builder.AddAttribute(10, "step", Step);
        builder.AddAttribute(11, "placeholder", Placeholder);
        builder.AddAttribute(12, "value", CurrentValueAsString);
        builder.AddMultipleAttributes(13, AdditionalAttributes);
        builder.AddAttribute(14, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInput));
        builder.CloseElement();

        builder.CloseElement();
    }
}
