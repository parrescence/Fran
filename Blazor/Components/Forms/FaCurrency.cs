using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

/// <summary>
/// Numeric amount field that shows a formatted "$ 12.34" while unfocused and a plain
/// editable number while focused. The library this was ported from did the same
/// focus/blur swap with two parallel &lt;input&gt; elements, an IJSRuntime call to
/// show/hide them, and a cursor-position workaround; here it's one &lt;input&gt; whose
/// <c>type</c>/<c>value</c> Blazor itself flips between "number"/raw and "text"/
/// formatted on <c>onfocus</c>/<c>onblur</c> — no JS at all.
/// </summary>
public sealed class FaCurrency : InputBase<decimal?>
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }
    [Parameter] public decimal? Min { get; set; }
    [Parameter] public decimal? Max { get; set; }
    [Parameter] public string CurrencySymbol { get; set; } = "$";
    /// <summary>Same boxed look as normal, just muted and non-interactive.</summary>
    [Parameter] public bool ReadOnly { get; set; }

    private readonly string _id = $"fa-currency-{Guid.NewGuid():N}";
    private bool _isEditing;

    protected override bool TryParseValueFromString(string? value, out decimal? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            validationErrorMessage = null;
            return true;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            result = Math.Round(parsed, 2, MidpointRounding.AwayFromZero);
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = $"'{value}' is not a valid amount.";
        return false;
    }

    private void HandleInput(ChangeEventArgs e)
    {
        CurrentValueAsString = e.Value?.ToString();

        if (Value is { } value)
        {
            if (Min is { } min && value < min)
            {
                CurrentValue = min;
            }
            else if (Max is { } max && value > max)
            {
                CurrentValue = max;
            }
        }
    }

    private void HandleFocus()
    {
        if (!ReadOnly)
        {
            _isEditing = true;
        }
    }
    private void HandleBlur() => _isEditing = false;

    private string DisplayText => Value is { } value
        ? $"{CurrencySymbol} {value.ToString("F2", CultureInfo.InvariantCulture)}"
        : string.Empty;

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
        builder.AddAttribute(8, "class", "fa-input fa-currency-input");
        builder.AddAttribute(9, "type", _isEditing ? "number" : "text");
        builder.AddAttribute(10, "inputmode", "decimal");
        builder.AddAttribute(11, "step", "0.01");
        builder.AddAttribute(12, "min", Min);
        builder.AddAttribute(13, "max", Max);
        builder.AddAttribute(14, "value", _isEditing ? CurrentValueAsString : DisplayText);
        builder.AddAttribute(15, "readonly", ReadOnly);
        builder.AddMultipleAttributes(16, AdditionalAttributes);
        builder.AddAttribute(17, "onfocus", EventCallback.Factory.Create<FocusEventArgs>(this, HandleFocus));
        builder.AddAttribute(18, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this, HandleBlur));
        builder.AddAttribute(19, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleInput));
        builder.CloseElement();

        builder.CloseElement();
    }
}
