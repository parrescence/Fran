using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

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

    private readonly string _id = $"fa-textarea-{Guid.NewGuid():N}";

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

        if (ReadOnlyDisplay)
        {
            builder.OpenElement(6, "div");
            builder.AddAttribute(7, "id", _id);
            builder.AddAttribute(8, "class", "fa-textarea fa-textarea-readonly");
            builder.AddMultipleAttributes(9, AdditionalAttributes);
            builder.AddContent(10, CurrentValueAsString);
            builder.CloseElement();
        }
        else
        {
            builder.OpenElement(11, "textarea");
            builder.AddAttribute(12, "id", _id);
            builder.AddAttribute(13, "class", "fa-textarea");
            builder.AddAttribute(14, "placeholder", Placeholder);
            builder.AddAttribute(15, "maxlength", MaxLength);
            builder.AddAttribute(16, "value", CurrentValueAsString);
            builder.AddMultipleAttributes(17, AdditionalAttributes);
            builder.AddAttribute(18, "oninput", EventCallback.Factory.CreateBinder<string?>(this, value => CurrentValueAsString = value, CurrentValueAsString));
            builder.SetUpdatesAttributeName("value");
            builder.AddElementReferenceCapture(19, reference => Element = reference);
            builder.CloseElement();

            if (MaxLength is int max)
            {
                builder.OpenElement(20, "div");
                builder.AddAttribute(21, "class", "fa-textarea-counter");
                builder.AddContent(22, $"{(CurrentValueAsString ?? string.Empty).Length}/{max}");
                builder.CloseElement();
            }
        }

        builder.CloseElement();
    }
}
