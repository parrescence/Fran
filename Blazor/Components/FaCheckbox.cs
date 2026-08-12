using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// Checkbox with an optional clickable label. Wrapping the input and label text in a
/// single &lt;label&gt; element gets click-to-toggle on the label for free from the
/// browser — no separate onclick handler toggling the value by hand.
/// </summary>
public sealed class FaCheckbox : InputCheckbox
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "label");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-checkbox", ContainerCssClass));

        builder.OpenElement(2, "input");
        builder.AddAttribute(3, "type", "checkbox");
        builder.AddAttribute(4, "class", "fa-checkbox-input");
        builder.AddAttribute(5, "checked", BindConverter.FormatValue(CurrentValue));
        builder.AddMultipleAttributes(6, AdditionalAttributes);
        builder.AddAttribute(7, "onchange", EventCallback.Factory.CreateBinder<bool>(this, value => CurrentValue = value, CurrentValue));
        builder.SetUpdatesAttributeName("checked");
        builder.AddElementReferenceCapture(8, reference => Element = reference);
        builder.CloseElement();

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(9, "span");
            builder.AddAttribute(10, "class", "fa-checkbox-label");
            builder.AddContent(11, Label);
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
