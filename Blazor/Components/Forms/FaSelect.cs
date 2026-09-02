using System.Diagnostics.CodeAnalysis;
using FaFa.Rendering;
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

    /// <summary>XSmall/Small/Medium(default)/Large/XLarge — see <see cref="FaSize"/>.</summary>
    [Parameter] public FaSize Size { get; set; } = FaSize.Medium;

    /// <summary>Stretches to 100% width below the 720px breakpoint (<c>.fa-responsive</c> in <c>_responsive.scss</c>). Off by default.</summary>
    [Parameter] public bool Responsive { get; set; }

    /// <summary>Shows this field's own EditContext validation errors (model/form-tier — see FaFa.Validation) inline below it, on by default — opt out per-instance for a layout that shows errors somewhere else instead.</summary>
    [Parameter] public bool ShowValidationMessage { get; set; } = true;

    /// <summary>Element-tier validation override — evaluated fresh every render against the current value, independent of whatever the model/form-tier validators say for this field; a non-null return always shows in addition to those.</summary>
    [Parameter] public Func<TValue, string?>? Validate { get; set; }

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
        var seq = 0;
        var messages = FaValidationMessageRenderer.Resolve(EditContext, FieldIdentifier, ShowValidationMessage, Validate?.Invoke(CurrentValue!));

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

        builder.OpenElement(seq++, "select");
        builder.AddAttribute(seq++, "id", _id);
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-select", FaSizeClassNames.Class("fa-input", Size), Responsive ? "fa-responsive" : null, messages.Count > 0 ? "fa-select-invalid" : null));
        builder.AddAttribute(seq++, "value", CurrentValueAsString);
        builder.AddAttribute(seq++, "disabled", ReadOnly);
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);
        builder.AddAttribute(seq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnChange));
        builder.AddContent(seq++, ChildContent);
        builder.CloseElement();

        // See FaInput.cs's own remarks on why this conditional block is wrapped in
        // OpenRegion rather than a bare seq++.
        builder.OpenRegion(seq++);
        FaValidationMessageRenderer.Render(builder, messages);
        builder.CloseRegion();

        builder.CloseElement();
    }
}
