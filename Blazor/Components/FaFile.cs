using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// File picker. Set <see cref="AsButton"/> to show a styled button (a &lt;label&gt;
/// wired to a visually-hidden &lt;InputFile&gt; via a stable id) instead of the
/// browser's default file input chrome.
/// </summary>
public sealed class FaFile : ComponentBase
{
    [Parameter] public EventCallback<InputFileChangeEventArgs> OnChange { get; set; }
    [Parameter] public bool AsButton { get; set; }
    [Parameter] public string ButtonLabel { get; set; } = "Choose file";
    [Parameter] public bool Multiple { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private readonly string _id = $"fa-file-{Guid.NewGuid():N}";

    private Task HandleChange(InputFileChangeEventArgs e) => OnChange.InvokeAsync(e);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (AsButton)
        {
            builder.OpenElement(0, "label");
            builder.AddAttribute(1, "for", _id);
            builder.AddAttribute(2, "class", CssClassNames.Combine("fa-btn", "fa-btn-secondary", CssClass));
            builder.AddContent(3, ButtonLabel);
            builder.CloseElement();
        }

        builder.OpenComponent<InputFile>(10);
        builder.AddComponentParameter(11, "id", _id);
        builder.AddComponentParameter(12, "class", AsButton ? "fa-file-hidden" : CssClassNames.Combine("fa-file-input", CssClass));
        builder.AddComponentParameter(13, "multiple", Multiple);
        builder.AddComponentParameter(14, nameof(InputFile.OnChange), EventCallback.Factory.Create<InputFileChangeEventArgs>(this, HandleChange));
        builder.CloseComponent();
    }
}
