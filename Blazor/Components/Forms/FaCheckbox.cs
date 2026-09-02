using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Components;

/// <summary>
/// Checkbox with an optional clickable label. Wrapping the input and label text in a
/// single &lt;label&gt; element gets click-to-toggle on the label for free from the
/// browser — no separate onclick handler toggling the value by hand.
/// </summary>
public sealed class FaCheckbox : InputCheckbox
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }
    /// <summary>
    /// Flattens to a plain checked/unchecked indicator with a bottom border instead
    /// of a clickable checkbox — the shared "other fa styles" readonly look (see
    /// FaInput/FaSelect/etc. for the boxed-muted style used by native inputs).
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>XSmall/Small/Medium(default)/Large/XLarge — scales the box and label together. See <see cref="FaSize"/>.</summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    /// <summary>Shows this field's own EditContext validation errors (model/form-tier — see FaFa.Validation) inline below it, on by default — opt out per-instance for a layout that shows errors somewhere else instead.</summary>
    [Parameter] public bool ShowValidationMessage { get; set; } = true;

    /// <summary>Element-tier validation override — evaluated fresh every render against the current value, independent of whatever the model/form-tier validators say for this field; a non-null return always shows in addition to those.</summary>
    [Parameter] public Func<bool, string?>? Validate { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // A running seq counter, not this file's old fixed literals — see
        // FaInput.cs's own remarks on why a conditionally-rendered
        // validation-message block needs one. No single wrapping element exists
        // here the way .fa-field wraps other inputs (a checkbox's own root is
        // either the readonly div or the label, never both, never a shared
        // parent) — Blazor render output doesn't require exactly one root
        // element, so the message below is just a second top-level sibling,
        // same either way.
        var seq = 0;
        var messages = FaValidationMessageRenderer.Resolve(EditContext, FieldIdentifier, ShowValidationMessage, Validate?.Invoke(CurrentValue));

        if (ReadOnly)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-checkbox-readonly", "fa-readonly-flat", ContainerCssClass));
            builder.AddContent(seq++, CurrentValue ? "☑ " : "☐ ");
            builder.AddContent(seq++, Label);
            builder.CloseElement();

            builder.OpenRegion(seq++);
            FaValidationMessageRenderer.Render(builder, messages);
            builder.CloseRegion();
            return;
        }

        builder.OpenElement(seq++, "label");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-checkbox", FaSizeClassNames.Class("fa-checkbox", Size), ContainerCssClass));

        builder.OpenElement(seq++, "input");
        builder.AddAttribute(seq++, "type", "checkbox");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-checkbox-input", messages.Count > 0 ? "fa-input-invalid" : null));
        builder.AddAttribute(seq++, "checked", BindConverter.FormatValue(CurrentValue));
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);
        builder.AddAttribute(seq++, "onchange", EventCallback.Factory.CreateBinder<bool>(this, value => CurrentValue = value, CurrentValue));
        builder.SetUpdatesAttributeName("checked");
        builder.AddElementReferenceCapture(seq++, reference => Element = reference);
        builder.CloseElement();

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "fa-checkbox-label");
            builder.AddContent(seq++, Label);
            builder.CloseElement();
        }

        builder.CloseElement();

        builder.OpenRegion(seq++);
        FaValidationMessageRenderer.Render(builder, messages);
        builder.CloseRegion();
    }
}
