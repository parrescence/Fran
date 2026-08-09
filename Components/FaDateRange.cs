using FactoryAspects.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FactoryAspects.Components;

/// <summary>
/// Two linked date fields (From/To) for range filters — e.g. "show transactions
/// between X and Y". Plain native &lt;input type="date"&gt; elements styled with the
/// same .fa-input class FaInput/FaSelect use, rather than a pair of FaDatePicker
/// popups — picking each end's date is a browser-native concern; this component's
/// job is keeping the two ends coherent (editing From past the current To pushes To
/// forward to match, and vice versa, rather than silently rejecting the edit) and
/// exposing one bound <see cref="FaDateRangeValue"/>.
/// </summary>
/// <remarks>
/// Not InputBase-based — a from/to pair has no single string round-trip the way
/// InputBase&lt;TValue&gt; expects (see FaInput/FaSelect/FaDatePicker). This follows the
/// plain two-way-bindable-parameter pattern FaToggle uses instead: <c>@bind-Value</c>
/// works the same way.
/// </remarks>
public sealed class FaDateRange : ComponentBase
{
    [Parameter] public FaDateRangeValue Value { get; set; }
    [Parameter] public EventCallback<FaDateRangeValue> ValueChanged { get; set; }

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? FromLabel { get; set; } = "From";
    [Parameter] public string? ToLabel { get; set; } = "To";
    /// <summary>Earliest selectable date for either end. Optional.</summary>
    [Parameter] public DateOnly? Min { get; set; }
    /// <summary>Latest selectable date for either end. Optional.</summary>
    [Parameter] public DateOnly? Max { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }

    private readonly string _fromId = $"fa-daterange-from-{Guid.NewGuid():N}";
    private readonly string _toId = $"fa-daterange-to-{Guid.NewGuid():N}";

    private const string DateFormat = "yyyy-MM-dd";

    private Task SetFromAsync(ChangeEventArgs e)
    {
        var parsed = DateOnly.TryParse(e.Value?.ToString(), out var date) ? date : (DateOnly?)null;
        var to = Value.To;
        if (parsed is { } from && to is { } currentTo && from > currentTo)
        {
            to = from;
        }
        return SetAsync(new FaDateRangeValue(parsed, to));
    }

    private Task SetToAsync(ChangeEventArgs e)
    {
        var parsed = DateOnly.TryParse(e.Value?.ToString(), out var date) ? date : (DateOnly?)null;
        var from = Value.From;
        if (parsed is { } to && from is { } currentFrom && to < currentFrom)
        {
            from = to;
        }
        return SetAsync(new FaDateRangeValue(from, parsed));
    }

    private async Task SetAsync(FaDateRangeValue value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ClassNames.Combine("fa-field", ContainerCssClass));

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(2, "label");
            builder.AddAttribute(3, "class", "fa-label");
            builder.AddContent(4, Label);
            builder.CloseElement();
        }

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "class", "fa-daterange");

        RenderEnd(builder, 10, _fromId, FromLabel, Value.From, Min, Value.To ?? Max, SetFromAsync);

        builder.OpenElement(30, "span");
        builder.AddAttribute(31, "class", "fa-daterange-separator");
        builder.AddAttribute(32, "aria-hidden", "true");
        builder.AddContent(33, "–");
        builder.CloseElement();

        RenderEnd(builder, 40, _toId, ToLabel, Value.To, Value.From ?? Min, Max, SetToAsync);

        builder.CloseElement();
        builder.CloseElement();
    }

    private void RenderEnd(RenderTreeBuilder builder, int sequence, string id, string? label, DateOnly? value, DateOnly? min, DateOnly? max, Func<ChangeEventArgs, Task> onChange)
    {
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "fa-daterange-field");

        if (!string.IsNullOrEmpty(label))
        {
            builder.OpenElement(sequence + 2, "label");
            builder.AddAttribute(sequence + 3, "class", "fa-daterange-end-label");
            builder.AddAttribute(sequence + 4, "for", id);
            builder.AddContent(sequence + 5, label);
            builder.CloseElement();
        }

        builder.OpenElement(sequence + 6, "input");
        builder.AddAttribute(sequence + 7, "id", id);
        builder.AddAttribute(sequence + 8, "class", "fa-input fa-daterange-input");
        builder.AddAttribute(sequence + 9, "type", "date");
        builder.AddAttribute(sequence + 10, "value", value?.ToString(DateFormat));
        if (min is { } minDate)
        {
            builder.AddAttribute(sequence + 11, "min", minDate.ToString(DateFormat));
        }
        if (max is { } maxDate)
        {
            builder.AddAttribute(sequence + 12, "max", maxDate.ToString(DateFormat));
        }
        builder.AddAttribute(sequence + 13, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, onChange));
        builder.CloseElement();

        builder.CloseElement();
    }
}
