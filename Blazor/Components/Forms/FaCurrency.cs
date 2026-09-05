using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Fran.Components;

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

    /// <summary>XSmall/Small/Medium(default)/Large/XLarge — see <see cref="FaSize"/>.</summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    /// <summary>Stretches to 100% width below the 720px breakpoint (<c>.fa-responsive</c> in <c>_responsive.scss</c>). Off by default.</summary>
    [Parameter] public bool Responsive { get; set; }

    private readonly string _id = $"fa-currency-{Guid.NewGuid():N}";
    private bool _isEditing;

    // A developer-configuration mistake, not user input — same reasoning as
    // FaDate's own _rangeError (see its OnParametersSet remarks): Min/Max are
    // plain parameters with no FieldIdentifier, so this is shown directly rather
    // than routed through Fran.Validation, reusing that system's
    // .fa-validation-message look without its machinery. Doesn't throw for the
    // same "Min/Max might be transiently bad while data's still loading" reason.
    private string? _rangeError;

    protected override void OnParametersSet()
    {
        _rangeError = Min is { } rangeMin && Max is { } rangeMax && rangeMax < rangeMin
            ? $"Max ({rangeMax:F2}) must be greater than or equal to Min ({rangeMin:F2})."
            : null;
    }

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
        var seq = 0;

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", $"fa-field {ContainerCssClass}");

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "class", "fa-label");
            builder.AddAttribute(seq++, "for", _id);
            builder.AddContent(seq++, Label);
            builder.CloseElement();
        }

        // Reuses Fran.Validation's own .fa-validation-message look without going
        // through that system — see _rangeError's own remarks. OpenRegion isolates
        // this conditional block the same way FaDate.cs's identical block does, so
        // Min/Max flipping between valid and invalid can't shift the input right
        // after it out from under Blazor's diff.
        builder.OpenRegion(seq++);
        if (_rangeError is not null)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "fa-validation-message");
            builder.AddContent(2, _rangeError);
            builder.CloseElement();
        }

        builder.CloseRegion();

        builder.OpenElement(seq++, "input");
        builder.AddAttribute(seq++, "id", _id);
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-input", "fa-currency-input", FaSizeClassNames.Class("fa-input", Size), Responsive ? "fa-responsive" : null));
        builder.AddAttribute(seq++, "type", _isEditing ? "number" : "text");
        builder.AddAttribute(seq++, "inputmode", "decimal");
        builder.AddAttribute(seq++, "step", "0.01");
        builder.AddAttribute(seq++, "min", Min);
        builder.AddAttribute(seq++, "max", Max);
        builder.AddAttribute(seq++, "value", _isEditing ? CurrentValueAsString : DisplayText);
        builder.AddAttribute(seq++, "readonly", ReadOnly);
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);
        builder.AddAttribute(seq++, "onfocus", EventCallback.Factory.Create<FocusEventArgs>(this, HandleFocus));
        builder.AddAttribute(seq++, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this, HandleBlur));
        builder.AddAttribute(seq++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleInput));
        builder.CloseElement();

        builder.CloseElement();
    }
}
