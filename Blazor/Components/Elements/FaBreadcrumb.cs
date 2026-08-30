using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Components;

/// <summary>
/// Path trail — Home &gt; Section &gt; Current page. The last item always renders as
/// plain text with aria-current="page" even if it has an Href, since linking to the
/// page you're already on isn't real navigation; every earlier item with a null/
/// empty Href renders as plain text too instead of a dead link.
/// </summary>
public sealed class FaBreadcrumb : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<(string Text, string? Href)> Items { get; set; } = Array.Empty<(string, string?)>();
    [Parameter] public string? CssClass { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-breadcrumb-nav", CssClass));
        builder.AddAttribute(2, "aria-label", "Breadcrumb");

        builder.OpenElement(3, "ol");
        builder.AddAttribute(4, "class", "fa-breadcrumb");

        var seq = 5;
        for (var i = 0; i < Items.Count; i++)
        {
            var (text, href) = Items[i];
            var isLast = i == Items.Count - 1;

            builder.OpenElement(seq++, "li");
            builder.SetKey(i);
            builder.AddAttribute(seq++, "class", "fa-breadcrumb-item");

            if (isLast || string.IsNullOrEmpty(href))
            {
                builder.OpenElement(seq++, "span");
                builder.AddAttribute(seq++, "aria-current", isLast ? "page" : null);
                builder.AddContent(seq++, text);
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(seq++, "a");
                builder.AddAttribute(seq++, "href", href);
                builder.AddContent(seq++, text);
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.CloseElement();
        builder.CloseElement();
    }
}
