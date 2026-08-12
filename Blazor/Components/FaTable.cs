using FactoryAspects.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Components;

/// <summary>
/// Renders a real &lt;table&gt;/&lt;thead&gt;/&lt;tbody&gt; from typed data — pass
/// <see cref="Columns"/> (header text, defines column order) and <see cref="Rows"/>
/// (one dictionary per row, keyed by column name, each value a
/// <see cref="RenderFragment"/> so a cell can hold arbitrary content: text, another
/// component, a formatted number, ...). Renders through <c>.fa-table</c> in
/// theme.css, which already styles plain &lt;table&gt; markup — this component exists
/// so callers get typed headers/rows instead of hand-writing that markup themselves.
/// Each generated &lt;td&gt; carries a <c>data-label</c> attribute (its column name),
/// which theme.css's &lt;=720px card mode uses to print a label next to the value —
/// callers get responsive card layout with no markup changes of their own. Hand-written
/// <c>&lt;table class="fa-table"&gt;</c> markup opts into card mode by adding
/// <c>data-label</c> to its own &lt;td&gt;s.
/// </summary>
public sealed class FaTable : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<string> Columns { get; set; } = Array.Empty<string>();
    [Parameter, EditorRequired] public IReadOnlyList<IReadOnlyDictionary<string, RenderFragment>> Rows { get; set; } = Array.Empty<IReadOnlyDictionary<string, RenderFragment>>();
    [Parameter] public string? CssClass { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "table");
        builder.AddAttribute(1, "class", ClassNames.Combine("fa-table", CssClass));
        builder.AddMultipleAttributes(2, AdditionalAttributes);

        builder.OpenElement(3, "thead");
        builder.OpenElement(4, "tr");
        var seq = 5;
        foreach (var column in Columns)
        {
            builder.OpenElement(seq++, "th");
            builder.AddContent(seq++, column);
            builder.CloseElement();
        }
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(seq++, "tbody");
        foreach (var row in Rows)
        {
            builder.OpenElement(seq++, "tr");
            foreach (var column in Columns)
            {
                builder.OpenElement(seq++, "td");
                builder.AddAttribute(seq++, "data-label", column);
                if (row.TryGetValue(column, out var cell))
                {
                    builder.AddContent(seq++, cell);
                }
                builder.CloseElement();
            }
            builder.CloseElement();
        }
        builder.CloseElement();

        builder.CloseElement();
    }
}
