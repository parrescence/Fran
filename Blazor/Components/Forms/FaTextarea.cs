using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// Multi-line text field. Set <see cref="MaxLength"/> to show a live character
/// counter, or <see cref="ReadOnlyDisplay"/> to render the value as a plain block
/// (matching surrounding text flow) instead of a boxed, scrollable textarea — for
/// pages that show a value without ever letting it be edited.
/// </summary>
public sealed class FaTextarea : InputTextArea
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }
    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public bool ReadOnlyDisplay { get; set; }
    /// <summary>
    /// Same boxed textarea look as normal, just muted and non-interactive — for a
    /// value that's temporarily locked but should still read as a form field.
    /// Different from <see cref="ReadOnlyDisplay"/>, which drops the box entirely to
    /// flow as plain text; this keeps the box. Ignored when <see cref="ReadOnlyDisplay"/>
    /// is also set (that one wins).
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>XSmall/Small/Medium(default)/Large/XLarge — see <see cref="FaSize"/>.</summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    /// <summary>Stretches to 100% width below the 720px breakpoint (<c>.fa-responsive</c> in <c>_responsive.scss</c>). Off by default.</summary>
    [Parameter] public bool Responsive { get; set; }

    /// <summary>Shows this field's own EditContext validation errors (model/form-tier — see Fran.Validation) inline below it, on by default — opt out per-instance for a layout that shows errors somewhere else instead.</summary>
    [Parameter] public bool ShowValidationMessage { get; set; } = true;

    /// <summary>Element-tier validation override — evaluated fresh every render against the current value, independent of whatever the model/form-tier validators say for this field; a non-null return always shows in addition to those.</summary>
    [Parameter] public Func<string?, string?>? Validate { get; set; }

    private readonly string _id = $"fa-textarea-{Guid.NewGuid():N}";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // A running seq counter (rather than the fixed literals this file used to
        // reuse across its two ReadOnlyDisplay branches) so a validation-message
        // block that only sometimes renders can't shift a fixed-numbered sibling's
        // identity out from under it — see FaInput.cs's own remarks on the same
        // OpenRegion pattern used below for why that matters.
        var seq = 0;
        var messages = FaValidationMessageRenderer.Resolve(EditContext, FieldIdentifier, ShowValidationMessage, Validate?.Invoke(CurrentValue));

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-field", ContainerCssClass));

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "class", "fa-label");
            builder.AddAttribute(seq++, "for", _id);
            builder.AddContent(seq++, Label);
            builder.CloseElement();
        }

        builder.OpenRegion(seq++);
        var regionSeq = 0;
        if (ReadOnlyDisplay)
        {
            builder.OpenElement(regionSeq++, "div");
            builder.AddAttribute(regionSeq++, "id", _id);
            builder.AddAttribute(regionSeq++, "class", "fa-textarea fa-textarea-readonly");
            builder.AddMultipleAttributes(regionSeq++, AdditionalAttributes);
            builder.AddContent(regionSeq++, CurrentValueAsString);
            builder.CloseElement();
        }
        else
        {
            builder.OpenElement(regionSeq++, "textarea");
            builder.AddAttribute(regionSeq++, "id", _id);
            builder.AddAttribute(regionSeq++, "class", CssClassNames.Combine("fa-textarea", FaSizeClassNames.Class("fa-input", Size), Responsive ? "fa-responsive" : null, messages.Count > 0 ? "fa-input-invalid" : null));
            builder.AddAttribute(regionSeq++, "placeholder", Placeholder);
            builder.AddAttribute(regionSeq++, "maxlength", MaxLength);
            builder.AddAttribute(regionSeq++, "value", CurrentValueAsString);
            builder.AddAttribute(regionSeq++, "readonly", ReadOnly);
            builder.AddMultipleAttributes(regionSeq++, AdditionalAttributes);
            builder.AddAttribute(regionSeq++, "oninput", EventCallback.Factory.CreateBinder<string?>(this, value => CurrentValueAsString = value, CurrentValueAsString));
            builder.SetUpdatesAttributeName("value");
            builder.AddElementReferenceCapture(regionSeq++, reference => Element = reference);
            builder.CloseElement();

            if (MaxLength is int max)
            {
                builder.OpenElement(regionSeq++, "div");
                builder.AddAttribute(regionSeq++, "class", "fa-textarea-counter");
                builder.AddContent(regionSeq++, $"{(CurrentValueAsString ?? string.Empty).Length}/{max}");
                builder.CloseElement();
            }
        }

        builder.CloseRegion();

        builder.OpenRegion(seq++);
        FaValidationMessageRenderer.Render(builder, messages);
        builder.CloseRegion();

        builder.CloseElement();
    }
}
