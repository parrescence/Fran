using FactoryAspects.Internal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FactoryAspects.Components;

/// <summary>
/// Type-to-search combobox: a text field that re-queries <see cref="QueryAsync"/>
/// (debounced) as the caller types, shows the results in a dropdown, and commits
/// <see cref="Value"/> when one is picked. <see cref="QueryAsync"/> is the caller's
/// own lookup — an in-memory filter over a local list, an API call, whatever — so
/// this component stays data-source-agnostic the same way FaTable takes typed
/// Columns/Rows instead of owning a data model.
/// </summary>
/// <remarks>
/// Not InputBase-based — the bound <see cref="Value"/> is an arbitrary
/// <typeparamref name="TItem"/>, not a string, so there's no
/// TryParseValueFromString round-trip to hook into (see FaToggle for the same
/// reasoning). The dropdown's open/close and "close when focus leaves the control"
/// behavior follows FaDatePicker's pattern — a bool field plus a
/// <c>@onfocusout</c>-with-grace-period, no JS interop.
/// </remarks>
public sealed class FaSearchSelect<TItem> : ComponentBase
{
    /// <summary>Looks up results for the current search text. Called debounced, not on every keystroke.</summary>
    [Parameter, EditorRequired] public Func<string, Task<IReadOnlyList<TItem>>> QueryAsync { get; set; } = null!;
    /// <summary>Text shown for an item, both in the dropdown and in the field once selected.</summary>
    [Parameter, EditorRequired] public Func<TItem, string> ItemLabel { get; set; } = null!;
    /// <summary>Optional custom rendering for a dropdown row; falls back to plain <see cref="ItemLabel"/> text.</summary>
    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }

    [Parameter] public TItem? Value { get; set; }
    [Parameter] public EventCallback<TItem?> ValueChanged { get; set; }

    [Parameter] public string? SearchText { get; set; }
    [Parameter] public EventCallback<string?> SearchTextChanged { get; set; }

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string NoResultsText { get; set; } = "No results";
    /// <summary>Search text shorter than this doesn't trigger a query. Default 1 (any non-empty text).</summary>
    [Parameter] public int MinQueryLength { get; set; } = 1;
    [Parameter] public int DebounceMilliseconds { get; set; } = 250;
    [Parameter] public string? ContainerCssClass { get; set; }

    private readonly string _id = $"fa-searchselect-{Guid.NewGuid():N}";

    private IReadOnlyList<TItem> _results = Array.Empty<TItem>();
    private bool _isOpen;
    private bool _isLoading;
    private int _highlightedIndex = -1;
    private CancellationTokenSource? _debounce;
    private CancellationTokenSource? _pendingClose;

    private async Task OnInputAsync(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString();
        await SearchTextChanged.InvokeAsync(SearchText);

        _debounce?.Cancel();
        var cts = new CancellationTokenSource();
        _debounce = cts;

        if ((SearchText?.Length ?? 0) < MinQueryLength)
        {
            _results = Array.Empty<TItem>();
            _isOpen = false;
            _highlightedIndex = -1;
            return;
        }

        try
        {
            await Task.Delay(DebounceMilliseconds, cts.Token);
            await RunQueryAsync(SearchText ?? "", cts.Token);
        }
        catch (TaskCanceledException)
        {
            // Superseded by a newer keystroke — its own debounce will run the query.
        }
    }

    private async Task RunQueryAsync(string query, CancellationToken token)
    {
        _isLoading = true;
        _isOpen = true;
        StateHasChanged();

        var results = await QueryAsync(query);
        if (token.IsCancellationRequested)
        {
            return;
        }

        _results = results;
        _isLoading = false;
        _highlightedIndex = _results.Count > 0 ? 0 : -1;
        StateHasChanged();
    }

    private async Task SelectAsync(TItem item)
    {
        Value = item;
        await ValueChanged.InvokeAsync(item);

        SearchText = ItemLabel(item);
        await SearchTextChanged.InvokeAsync(SearchText);

        _isOpen = false;
        _results = Array.Empty<TItem>();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown" when _results.Count > 0:
                _isOpen = true;
                _highlightedIndex = Math.Min(_highlightedIndex + 1, _results.Count - 1);
                break;
            case "ArrowUp" when _results.Count > 0:
                _isOpen = true;
                _highlightedIndex = Math.Max(_highlightedIndex - 1, 0);
                break;
            case "Enter" when _isOpen && _highlightedIndex >= 0 && _highlightedIndex < _results.Count:
                await SelectAsync(_results[_highlightedIndex]);
                break;
            case "Escape":
                _isOpen = false;
                break;
        }
    }

    private void HandleFocusIn()
    {
        CancelPendingClose();
        if (_results.Count > 0 || _isLoading)
        {
            _isOpen = true;
        }
    }

    // Same grace-period pattern as FaDatePicker: focusout fires when focus leaves the
    // whole control, and a short delay lets a focusin on a sibling (e.g. moving focus
    // into the dropdown to click an option) cancel the pending close first.
    private async Task HandleFocusOutAsync()
    {
        CancelPendingClose();
        _pendingClose = new CancellationTokenSource();
        var token = _pendingClose.Token;
        try
        {
            await Task.Delay(150, token);
            _isOpen = false;
            StateHasChanged();
        }
        catch (TaskCanceledException)
        {
            // A focusin inside the wrapper cancelled the close — nothing to do.
        }
    }

    private void CancelPendingClose()
    {
        _pendingClose?.Cancel();
        _pendingClose = null;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ClassNames.Combine("fa-field", ContainerCssClass));

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(2, "label");
            builder.AddAttribute(3, "class", "fa-label");
            builder.AddAttribute(4, "for", _id);
            builder.AddContent(5, Label);
            builder.CloseElement();
        }

        builder.OpenElement(6, "div");
        builder.AddAttribute(7, "class", "fa-searchselect");
        builder.AddAttribute(8, "onfocusin", EventCallback.Factory.Create(this, HandleFocusIn));
        builder.AddAttribute(9, "onfocusout", EventCallback.Factory.Create(this, HandleFocusOutAsync));
        builder.AddAttribute(10, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));

        builder.OpenElement(11, "input");
        builder.AddAttribute(12, "id", _id);
        builder.AddAttribute(13, "class", "fa-input fa-searchselect-input");
        builder.AddAttribute(14, "type", "text");
        builder.AddAttribute(15, "role", "combobox");
        builder.AddAttribute(16, "aria-expanded", _isOpen ? "true" : "false");
        builder.AddAttribute(17, "autocomplete", "off");
        builder.AddAttribute(18, "placeholder", Placeholder);
        builder.AddAttribute(19, "value", SearchText);
        builder.AddAttribute(20, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInputAsync));
        builder.CloseElement();

        if (_isOpen)
        {
            RenderDropdown(builder, 30);
        }

        builder.CloseElement();
        builder.CloseElement();
    }

    private void RenderDropdown(RenderTreeBuilder builder, int sequence)
    {
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "fa-searchselect-dropdown");
        builder.AddAttribute(sequence + 2, "role", "listbox");

        if (_isLoading)
        {
            builder.OpenElement(sequence + 3, "div");
            builder.AddAttribute(sequence + 4, "class", "fa-searchselect-status");
            builder.AddContent(sequence + 5, "Searching…");
            builder.CloseElement();
        }
        else if (_results.Count == 0)
        {
            builder.OpenElement(sequence + 6, "div");
            builder.AddAttribute(sequence + 7, "class", "fa-searchselect-empty");
            builder.AddContent(sequence + 8, NoResultsText);
            builder.CloseElement();
        }
        else
        {
            var seq = sequence + 9;
            for (var i = 0; i < _results.Count; i++)
            {
                var index = i;
                var item = _results[index];
                var isHighlighted = index == _highlightedIndex;

                builder.OpenElement(seq++, "button");
                builder.SetKey(index);
                builder.AddAttribute(seq++, "type", "button");
                builder.AddAttribute(seq++, "class", ClassNames.Combine("fa-searchselect-option", isHighlighted ? "fa-searchselect-option-highlighted" : null));
                builder.AddAttribute(seq++, "role", "option");
                builder.AddAttribute(seq++, "aria-selected", isHighlighted ? "true" : "false");
                builder.AddAttribute(seq++, "onmouseenter", EventCallback.Factory.Create(this, () => _highlightedIndex = index));
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => SelectAsync(item)));
                if (ItemTemplate is not null)
                {
                    builder.AddContent(seq++, ItemTemplate(item));
                }
                else
                {
                    builder.AddContent(seq++, ItemLabel(item));
                }
                builder.CloseElement();
            }
        }

        builder.CloseElement();
    }
}
