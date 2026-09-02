using System.Diagnostics.CodeAnalysis;
using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

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
    /// <summary>Same boxed look as normal, just muted and non-interactive — the shared "input type" readonly style (see FaCheckbox/FaToggle/etc. for the flattened style used by non-native-input components).</summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>XSmall/Small/Medium(default)/Large/XLarge — see <see cref="FaSize"/>.</summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    /// <summary>Stretches to 100% width below the 720px breakpoint (<c>.fa-responsive</c> in <c>_responsive.scss</c>). Off by default.</summary>
    [Parameter] public bool Responsive { get; set; }

    /// <summary>
    /// Renders <see cref="Label"/> as a Material-style floating label overlaid inside
    /// the input instead of a block label above it — sits centered over the
    /// placeholder area until focused or filled, then animates up and shrinks to sit
    /// on the border. Ignored (falls back to the normal block label) when
    /// <see cref="Label"/> is null/empty, since there's nothing to float. See
    /// <c>_inputs.scss</c>'s <c>.fa-field-floating</c> for the mechanics — pure CSS,
    /// driven by <c>:focus</c>/<c>:placeholder-shown</c>, no JS.
    /// </summary>
    [Parameter] public bool FloatingLabel { get; set; }

    /// <summary>Shows this field's own EditContext validation errors (model/form-tier — see FaFa.Validation) inline below it, on by default — opt out per-instance for a layout that shows errors somewhere else instead (e.g. paired with a standalone FaValidationMessage placed elsewhere).</summary>
    [Parameter] public bool ShowValidationMessage { get; set; } = true;

    /// <summary>Element-tier validation override — evaluated fresh every render against the current value, independent of whatever the model/form-tier validators say for this field; a non-null return always shows in addition to those.</summary>
    [Parameter] public Func<TValue, string?>? Validate { get; set; }

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
        var seq = 0;
        var messages = FaValidationMessageRenderer.Resolve(EditContext, FieldIdentifier, ShowValidationMessage, Validate?.Invoke(CurrentValue!));
        var floating = FloatingLabel && !string.IsNullOrEmpty(Label);

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-field", floating ? "fa-field-floating" : null, ContainerCssClass));

        if (!floating && !string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "class", "fa-label");
            builder.AddAttribute(seq++, "for", _id);
            builder.AddContent(seq++, Label);
            builder.CloseElement();
        }

        builder.OpenElement(seq++, "input");
        builder.AddAttribute(seq++, "id", _id);
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-input", FaSizeClassNames.Class("fa-input", Size), Responsive ? "fa-responsive" : null, messages.Count > 0 ? "fa-input-invalid" : null));
        builder.AddAttribute(seq++, "type", Type);
        builder.AddAttribute(seq++, "step", Step);
        // Floating requires a non-empty placeholder for the :placeholder-shown CSS
        // trick below to work at all — a single space is invisible but still counts
        // as "has a placeholder" to the browser, so an explicit Placeholder (if any)
        // still shows normally when the field is empty and unfocused.
        builder.AddAttribute(seq++, "placeholder", floating ? (Placeholder ?? " ") : Placeholder);
        builder.AddAttribute(seq++, "value", CurrentValueAsString);
        builder.AddAttribute(seq++, "readonly", ReadOnly);
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);
        builder.AddAttribute(seq++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInput));
        builder.CloseElement();

        if (floating)
        {
            // After the <input>, not before — the CSS that floats this label on
            // focus/fill relies on the ~ general-sibling combinator (:focus ~ label),
            // which only matches a following sibling. See _inputs.scss's
            // .fa-field-floating block for the mechanics.
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "class", "fa-label fa-label-floating");
            builder.AddAttribute(seq++, "for", _id);
            builder.AddContent(seq++, Label);
            builder.CloseElement();
        }

        // OpenRegion isolates this conditional block's own sequence numbering behind
        // a single stable outer slot — with a variable presence/absence across
        // renders (no messages vs. some), a bare seq++ here would otherwise shift
        // whatever comes after it, and Blazor's diff would misread the shift as "the
        // old element's gone, here's an unrelated new one" the same way FaDate.cs's
        // own remarks describe for its calendar header/mode-toggle button. Nothing
        // follows this today, but the pattern only costs one extra line and avoids
        // a class of bug this library has already hit twice elsewhere.
        builder.OpenRegion(seq++);
        FaValidationMessageRenderer.Render(builder, messages);
        builder.CloseRegion();

        builder.CloseElement();
    }
}
