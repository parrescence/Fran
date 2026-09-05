using Fran.Icons;
using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// "Nothing here yet" placeholder for an empty table/grid/list — an icon, a title,
/// an optional description, and optional action content (typically a FaButton)
/// through ChildContent. Not tied to FaGrid/FaTable specifically; drop it in
/// wherever a collection came back empty.
/// </summary>
public sealed class FaEmptyState : ComponentBase
{
    [Parameter] public FaIconName IconName { get; set; } = FaIconName.Search;
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter] public string? Description { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? CssClass { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-empty-state", CssClass));

        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "fa-empty-state-icon");
        builder.OpenComponent<FaIcon>(4);
        builder.AddComponentParameter(5, nameof(FaIcon.Name), IconName);
        builder.AddComponentParameter(6, nameof(FaIcon.Color), FaIconColor.Black);
        builder.AddComponentParameter(7, nameof(FaIcon.Size), 28);
        builder.CloseComponent();
        builder.CloseElement();

        builder.OpenElement(8, "p");
        builder.AddAttribute(9, "class", "fa-empty-state-title");
        builder.AddContent(10, Title);
        builder.CloseElement();

        if (!string.IsNullOrEmpty(Description))
        {
            builder.OpenElement(11, "p");
            builder.AddAttribute(12, "class", "fa-empty-state-description");
            builder.AddContent(13, Description);
            builder.CloseElement();
        }

        if (ChildContent is not null)
        {
            builder.OpenElement(14, "div");
            builder.AddAttribute(15, "class", "fa-empty-state-actions");
            builder.AddContent(16, ChildContent);
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
