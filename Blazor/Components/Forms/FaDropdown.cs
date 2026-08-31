using FaFa.Icons;
using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

/// <summary>
/// Dropdown over a caller-supplied local <see cref="Items"/> list — or, when <see
/// cref="QueryPageAsync"/> is set, a server-paged one, in the same spirit as
/// <see cref="FaGrid{TItem}"/>'s <c>ItemsProvider</c> — in two selection modes
/// picked via <see cref="Searchable"/>: the default is a type-to-filter combobox
/// with a contains-text search, same spirit as <see cref="FaSearchSelect{TItem}"/>;
/// setting <see cref="Searchable"/> to <c>false</c> gives a plain click-to-open
/// dropdown instead — a read-only field that just lists the results, same
/// interaction shape as a native &lt;select&gt;. Either way, the dropdown never
/// renders more than <see cref="MaxVisibleItems"/> rows at once (default 10):
/// scrolling (mouse wheel, or arrow keys pushing the highlight past the visible
/// edge) slides a window over the results, swapping one row in and one row out,
/// rather than growing the dropdown or relying on native overflow scrolling — and
/// in remote mode, sliding the window past what's already been fetched pulls the
/// next page from <see cref="QueryPageAsync"/> instead of paging the whole list in
/// up front.
/// </summary>
/// <remarks>
/// Not InputBase-based — same reasoning as FaSearchSelect: the bound <see cref="Value"/>
/// is an arbitrary <typeparamref name="TItem"/>, not a string. Open/close and the
/// focus-out grace period follow the same FaDate-derived pattern as
/// FaSearchSelect — no JS interop.
/// </remarks>
public sealed class FaDropdown<TItem> : ComponentBase
{
    /// <summary>The full local list to filter/select from. Ignored when <see cref="QueryPageAsync"/> is set.</summary>
    [Parameter] public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();
    /// <summary>
    /// Looks up one page of results — <c>(search, skip, take)</c> in, <c>(items,
    /// totalCount)</c> out. Set this instead of <see cref="Items"/> to back the
    /// dropdown with a database/API call: an in-memory filter, a paged query,
    /// whatever — this component only ever asks for exactly the rows it needs to
    /// render (the current window, <see cref="MaxVisibleItems"/> at a time),
    /// re-querying from <c>skip: 0</c> (debounced) as the search text changes and
    /// fetching the next chunk only once scrolling reaches rows it hasn't loaded yet.
    /// The provider owns the actual filtering/paging — same "caller owns the data
    /// source" split as <see cref="FaSearchSelect{TItem}.QueryAsync"/> and
    /// <see cref="FaGrid{TItem}.ItemsProvider"/>.
    /// </summary>
    [Parameter] public Func<string, int, int, Task<(IReadOnlyList<TItem> Items, int TotalCount)>>? QueryPageAsync { get; set; }
    /// <summary>Text shown for an item, both in the dropdown and in the field once selected. In local mode, also what the contains-search matches against.</summary>
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
    [Parameter] public string LoadingText { get; set; } = "Loading…";
    /// <summary>How many rows the dropdown shows at once; scrolling slides the window instead of growing past this. Also the page size passed to <see cref="QueryPageAsync"/>. Default 10.</summary>
    [Parameter] public int MaxVisibleItems { get; set; } = 10;
    /// <summary>Debounce before a search-text change re-queries <see cref="QueryPageAsync"/>. Ignored in local mode.</summary>
    [Parameter] public int DebounceMilliseconds { get; set; } = 250;
    /// <summary>
    /// <c>true</c> (default) is a type-to-filter combobox — the field is editable and
    /// narrows the results as the caller types. <c>false</c> is a plain click-to-open
    /// dropdown — the field is read-only, always lists the full (windowed) result
    /// set, and toggles open/closed on click, the same interaction shape as a native
    /// &lt;select&gt;.
    /// </summary>
    [Parameter] public bool Searchable { get; set; } = true;
    /// <summary>
    /// Flattens the field to the selected item's label as plain text with a bottom
    /// border — the shared "other fa styles" readonly look (see FaInput/FaSelect/etc.
    /// for the boxed-muted style used by native inputs).
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }

    private bool IsRemote => QueryPageAsync is not null;

    private readonly string _id = $"fa-dropdown-{Guid.NewGuid():N}";

    // Local mode.
    private IReadOnlyList<TItem> _filtered = Array.Empty<TItem>();

    // Remote mode.
    private readonly List<TItem> _remoteItems = new();
    private int _remoteTotalCount;
    private bool _remoteLoadedOnce;
    private CancellationTokenSource? _debounce;

    private bool _isOpen;
    private bool _isLoading;
    private int _windowStart;
    private int _highlightedIndex = -1;

    private CancellationTokenSource? _pendingClose;

    private IReadOnlyList<TItem> CurrentItems => IsRemote ? _remoteItems : _filtered;
    private int CurrentCount => IsRemote ? _remoteTotalCount : _filtered.Count;
    private int MaxWindowStart => Math.Max(0, CurrentCount - MaxVisibleItems);

    private void ApplyLocalFilter()
    {
        var text = Searchable ? SearchText : null;
        _filtered = string.IsNullOrEmpty(text)
            ? Items
            : Items.Where(item => ItemLabel(item).Contains(text, StringComparison.OrdinalIgnoreCase)).ToArray();

        _windowStart = 0;
        _highlightedIndex = _filtered.Count > 0 ? 0 : -1;
    }

    private async Task LoadFirstRemotePageAsync(CancellationToken token = default)
    {
        _isLoading = true;
        StateHasChanged();

        var (items, total) = await QueryPageAsync!(Searchable ? SearchText ?? "" : "", 0, MaxVisibleItems);
        if (token.IsCancellationRequested)
        {
            return;
        }

        _remoteItems.Clear();
        _remoteItems.AddRange(items);
        _remoteTotalCount = total;
        _remoteLoadedOnce = true;
        _windowStart = 0;
        _highlightedIndex = _remoteItems.Count > 0 ? 0 : -1;
        _isLoading = false;
        StateHasChanged();
    }

    // Fetches the next chunk once the window (or the keyboard highlight) has slid
    // past what's already been loaded — the remote-mode equivalent of the local
    // window sliding over an in-memory list.
    private async Task EnsureWindowLoadedAsync()
    {
        if (!IsRemote || _isLoading)
        {
            return;
        }

        var needed = Math.Min(_windowStart + MaxVisibleItems, _remoteTotalCount);
        if (needed <= _remoteItems.Count)
        {
            return;
        }

        _isLoading = true;
        StateHasChanged();

        var (items, total) = await QueryPageAsync!(Searchable ? SearchText ?? "" : "", _remoteItems.Count, MaxVisibleItems);
        _remoteItems.AddRange(items);
        _remoteTotalCount = total;
        _isLoading = false;
        StateHasChanged();
    }

    private void OpenDropdown()
    {
        _isOpen = true;

        if (IsRemote)
        {
            if (!_remoteLoadedOnce)
            {
                _ = LoadFirstRemotePageAsync();
            }
        }
        else
        {
            ApplyLocalFilter();
        }
    }

    private async Task OnInputAsync(ChangeEventArgs e)
    {
        if (!Searchable)
        {
            return;
        }

        SearchText = e.Value?.ToString();
        await SearchTextChanged.InvokeAsync(SearchText);
        _isOpen = true;

        if (IsRemote)
        {
            _debounce?.Cancel();
            var cts = new CancellationTokenSource();
            _debounce = cts;
            try
            {
                await Task.Delay(DebounceMilliseconds, cts.Token);
                await LoadFirstRemotePageAsync(cts.Token);
            }
            catch (TaskCanceledException)
            {
                // Superseded by a newer keystroke — its own debounce will run the query.
            }
        }
        else
        {
            ApplyLocalFilter();
        }
    }

    // Only meaningful in non-searchable mode: the field is read-only, so clicking it
    // is the only way to open it, and clicking it again closes it — same toggle
    // interaction as a native <select>.
    private void HandleClick()
    {
        if (Searchable)
        {
            return;
        }

        CancelPendingClose();
        if (_isOpen)
        {
            _isOpen = false;
        }
        else
        {
            OpenDropdown();
        }
    }

    private async Task SelectAsync(TItem item)
    {
        Value = item;
        await ValueChanged.InvokeAsync(item);

        SearchText = ItemLabel(item);
        await SearchTextChanged.InvokeAsync(SearchText);

        _isOpen = false;
    }

    // Slides the visible window by one row per wheel tick instead of letting the
    // dropdown grow or natively scroll — only MaxVisibleItems rows are ever rendered.
    private void HandleWheel(WheelEventArgs e)
    {
        if (e.DeltaY > 0)
        {
            _windowStart = Math.Min(_windowStart + 1, MaxWindowStart);
        }
        else if (e.DeltaY < 0)
        {
            _windowStart = Math.Max(_windowStart - 1, 0);
        }

        _ = EnsureWindowLoadedAsync();
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown":
                if (!_isOpen)
                {
                    OpenDropdown();
                }
                if (CurrentCount > 0)
                {
                    _highlightedIndex = Math.Min(_highlightedIndex + 1, CurrentCount - 1);
                    EnsureHighlightedInWindow();
                }
                break;
            case "ArrowUp":
                if (!_isOpen)
                {
                    OpenDropdown();
                }
                if (CurrentCount > 0)
                {
                    _highlightedIndex = Math.Max(_highlightedIndex - 1, 0);
                    EnsureHighlightedInWindow();
                }
                break;
            case "Enter" when _isOpen && _highlightedIndex >= 0 && _highlightedIndex < CurrentItems.Count:
                _ = SelectAsync(CurrentItems[_highlightedIndex]);
                break;
            case "Escape":
                _isOpen = false;
                break;
        }
    }

    // Keeps the keyboard-highlighted row inside the rendered window by sliding the
    // window just far enough — same "swap one in, one out" motion as the wheel handler.
    private void EnsureHighlightedInWindow()
    {
        if (_highlightedIndex < _windowStart)
        {
            _windowStart = _highlightedIndex;
        }
        else if (_highlightedIndex >= _windowStart + MaxVisibleItems)
        {
            _windowStart = _highlightedIndex - MaxVisibleItems + 1;
        }

        _ = EnsureWindowLoadedAsync();
    }

    private void HandleFocusIn()
    {
        CancelPendingClose();

        // Searchable mode opens as soon as the field is focused, same as
        // FaSearchSelect. Non-searchable mode waits for an explicit click/arrow key
        // so that tabbing past it doesn't pop the list open unasked.
        if (Searchable)
        {
            OpenDropdown();
        }
    }

    // Same grace-period pattern as FaSearchSelect/FaDate: focusout fires when
    // focus leaves the whole control, and a short delay lets a focusin on a sibling
    // (e.g. moving focus into the dropdown to click an option) cancel the pending close.
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
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-field", ContainerCssClass));

        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(2, "label");
            builder.AddAttribute(3, "class", "fa-label");
            builder.AddAttribute(4, "for", _id);
            builder.AddContent(5, Label);
            builder.CloseElement();
        }

        if (ReadOnly)
        {
            builder.OpenElement(6, "div");
            builder.AddAttribute(7, "class", "fa-readonly-flat");
            builder.AddContent(8, Value is not null ? ItemLabel(Value) : SearchText);
            builder.CloseElement();
            builder.CloseElement();
            return;
        }

        builder.OpenElement(6, "div");
        builder.AddAttribute(7, "class", "fa-dropdown");
        builder.AddAttribute(8, "onfocusin", EventCallback.Factory.Create(this, HandleFocusIn));
        builder.AddAttribute(9, "onfocusout", EventCallback.Factory.Create(this, HandleFocusOutAsync));
        builder.AddAttribute(10, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDown));

        builder.OpenElement(11, "input");
        builder.AddAttribute(12, "id", _id);
        builder.AddAttribute(13, "class", "fa-input fa-dropdown-input");
        builder.AddAttribute(14, "type", "text");
        builder.AddAttribute(15, "role", "combobox");
        builder.AddAttribute(16, "aria-expanded", _isOpen ? "true" : "false");
        builder.AddAttribute(17, "autocomplete", "off");
        builder.AddAttribute(18, "placeholder", Placeholder);
        builder.AddAttribute(19, "value", SearchText);
        builder.AddAttribute(20, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInputAsync));
        builder.AddAttribute(21, "onclick", EventCallback.Factory.Create(this, HandleClick));
        if (!Searchable)
        {
            builder.AddAttribute(22, "readonly", true);
        }
        builder.CloseElement();

        // Floats over the input's end without stealing its click/type target — the
        // input itself still spans the full width underneath (see
        // .fa-dropdown-arrow's pointer-events: none), so typing to search is
        // unaffected. Flips chevron direction with _isOpen but otherwise always shows
        // one, closed or open.
        builder.OpenElement(23, "span");
        builder.AddAttribute(24, "class", "fa-dropdown-arrow");
        builder.AddAttribute(25, "aria-hidden", "true");
        builder.OpenComponent<FaIcon>(26);
        builder.AddComponentParameter(27, nameof(FaIcon.Name), _isOpen ? FaIconName.ChevronUp : FaIconName.ChevronDown);
        builder.AddComponentParameter(28, nameof(FaIcon.Color), FaIconColor.Black);
        builder.AddComponentParameter(29, nameof(FaIcon.Size), 16);
        builder.CloseComponent();
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
        builder.AddAttribute(sequence + 1, "class", "fa-dropdown-dropdown");
        builder.AddAttribute(sequence + 2, "role", "listbox");
        builder.AddAttribute(sequence + 3, "onwheel", EventCallback.Factory.Create<WheelEventArgs>(this, HandleWheel));
        builder.AddEventPreventDefaultAttribute(sequence + 4, "onwheel", true);

        if (_isLoading && CurrentItems.Count == 0)
        {
            builder.OpenElement(sequence + 5, "div");
            builder.AddAttribute(sequence + 6, "class", "fa-dropdown-status");
            builder.AddContent(sequence + 7, LoadingText);
            builder.CloseElement();
        }
        else if (CurrentItems.Count == 0)
        {
            builder.OpenElement(sequence + 5, "div");
            builder.AddAttribute(sequence + 6, "class", "fa-dropdown-empty");
            builder.AddContent(sequence + 7, NoResultsText);
            builder.CloseElement();
        }
        else
        {
            var seq = sequence + 8;
            var windowEnd = Math.Min(_windowStart + MaxVisibleItems, CurrentItems.Count);
            for (var index = _windowStart; index < windowEnd; index++)
            {
                var item = CurrentItems[index];
                var isHighlighted = index == _highlightedIndex;

                builder.OpenElement(seq++, "button");
                builder.SetKey(index);
                builder.AddAttribute(seq++, "type", "button");
                builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-dropdown-option", isHighlighted ? "fa-dropdown-option-highlighted" : null));
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
