using FactoryAspects.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// Circular avatar for the header's user chip — shows a photo when <see cref="ImageUrl"/>
/// is set, otherwise falls back to the user's initials on a warm-tint background.
/// </summary>
public sealed class FaAvatar : ComponentBase
{
    [Parameter] public string? DisplayName { get; set; }
    [Parameter] public string? ImageUrl { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                return "?";
            }

            var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpperInvariant(),
                _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
            };
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", ClassNames.Combine("fa-avatar", CssClass));
        builder.AddAttribute(2, "title", DisplayName);

        if (!string.IsNullOrWhiteSpace(ImageUrl))
        {
            builder.OpenElement(3, "img");
            builder.AddAttribute(4, "src", ImageUrl);
            builder.AddAttribute(5, "alt", DisplayName);
            builder.CloseElement();
        }
        else
        {
            builder.AddContent(6, Initials);
        }

        builder.CloseElement();
    }
}
