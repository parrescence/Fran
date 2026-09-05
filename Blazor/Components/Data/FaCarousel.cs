using Fran.Icons;
using Fran.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// A slideshow over a set of items — one slide visible at a time, with prev/next
/// arrows and dot navigation. Same "exactly one of Items/ItemsProvider" data-source
/// split as <see cref="FaGrid{TItem}"/>, just without paging (a carousel holds
/// every item, one at a time, rather than a page of them):
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b><see cref="Items"/></b> — an already-in-memory list, built however you
/// like: a <c>List&lt;T&gt;</c>/array literal written directly in code, or an
/// existing <c>Dictionary&lt;TKey, TItem&gt;</c>'s <c>.Values.ToList()</c>. FaCarousel
/// just reads it; nothing here cares where it came from.</item>
/// <item><b><see cref="ItemsProvider"/></b> — fetched once, asynchronously, when the
/// carousel first renders — a database/API call, without FaCarousel knowing or
/// caring which. There's no paging to re-fetch on; it's a single fetch of the
/// whole slide set.</item>
/// </list>
/// Each slide's markup comes from <see cref="ItemTemplate"/>. For hand-authored
/// slides that aren't backed by a real data object, use <c>IReadOnlyList&lt;
/// RenderFragment&gt;</c> as <typeparamref name="TItem"/> with
/// <c>ItemTemplate="frag =&gt; frag"</c> — that's "just pass in markup written in
/// code" without FaCarousel needing a separate mode for it.
/// </remarks>
public sealed class FaCarousel<TItem> : ComponentBase, IDisposable
{
    [Parameter] public IReadOnlyList<TItem>? Items { get; set; }
    [Parameter] public Func<Task<IReadOnlyList<TItem>>>? ItemsProvider { get; set; }
    [Parameter, EditorRequired] public RenderFragment<TItem> ItemTemplate { get; set; } = null!;

    [Parameter] public bool ShowArrows { get; set; } = true;
    [Parameter] public bool ShowDots { get; set; } = true;

    /// <summary>Advances to the next slide automatically every <see cref="AutoPlayIntervalMs"/>. Off by default.</summary>
    [Parameter] public bool AutoPlay { get; set; }
    [Parameter] public int AutoPlayIntervalMs { get; set; } = 4000;

    [Parameter] public string? CssClass { get; set; }

    private IReadOnlyList<TItem> _providerItems = Array.Empty<TItem>();
    private int _index;
    private Timer? _autoPlayTimer;

    private bool IsProviderMode => ItemsProvider is not null;
    private IReadOnlyList<TItem> ActiveItems => IsProviderMode ? _providerItems : Items ?? Array.Empty<TItem>();

    protected override void OnParametersSet()
    {
        var hasItems = Items is not null;
        var hasProvider = ItemsProvider is not null;
        if (hasItems == hasProvider)
        {
            throw new ArgumentException(
                $"FaCarousel requires exactly one of {nameof(Items)} or {nameof(ItemsProvider)} to be set.",
                hasItems ? nameof(ItemsProvider) : nameof(Items));
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (IsProviderMode)
        {
            await LoadAsync();
        }

        if (AutoPlay)
        {
            _autoPlayTimer = new Timer(_ => InvokeAsync(() =>
            {
                Next();
                StateHasChanged();
            }), null, AutoPlayIntervalMs, AutoPlayIntervalMs);
        }
    }

    private async Task LoadAsync()
    {
        if (ItemsProvider is null)
        {
            return;
        }

        _providerItems = await ItemsProvider();
        _index = 0;
    }

    private void GoTo(int index)
    {
        var count = ActiveItems.Count;
        _index = count == 0 ? 0 : ((index % count) + count) % count;
    }

    private void Previous() => GoTo(_index - 1);
    private void Next() => GoTo(_index + 1);

    public void Dispose() => _autoPlayTimer?.Dispose();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var items = ActiveItems;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-carousel", CssClass));

        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "fa-carousel-viewport");
        if (items.Count > 0)
        {
            builder.OpenElement(4, "div");
            builder.SetKey(_index);
            builder.AddAttribute(5, "class", "fa-carousel-slide");
            builder.AddContent(6, ItemTemplate(items[_index]));
            builder.CloseElement();
        }
        builder.CloseElement();

        if (ShowArrows && items.Count > 1)
        {
            builder.OpenElement(7, "button");
            builder.AddAttribute(8, "type", "button");
            builder.AddAttribute(9, "class", "fa-carousel-arrow fa-carousel-arrow-prev");
            builder.AddAttribute(10, "aria-label", "Previous slide");
            builder.AddAttribute(11, "onclick", EventCallback.Factory.Create(this, Previous));
            builder.OpenComponent<FaIcon>(12);
            builder.AddComponentParameter(13, nameof(FaIcon.Name), FaIconName.ChevronLeft);
            builder.AddComponentParameter(14, nameof(FaIcon.Color), FaIconColor.Black);
            builder.AddComponentParameter(15, nameof(FaIcon.Size), 16);
            builder.CloseComponent();
            builder.CloseElement();

            builder.OpenElement(16, "button");
            builder.AddAttribute(17, "type", "button");
            builder.AddAttribute(18, "class", "fa-carousel-arrow fa-carousel-arrow-next");
            builder.AddAttribute(19, "aria-label", "Next slide");
            builder.AddAttribute(20, "onclick", EventCallback.Factory.Create(this, Next));
            builder.OpenComponent<FaIcon>(21);
            builder.AddComponentParameter(22, nameof(FaIcon.Name), FaIconName.ChevronRight);
            builder.AddComponentParameter(23, nameof(FaIcon.Color), FaIconColor.Black);
            builder.AddComponentParameter(24, nameof(FaIcon.Size), 16);
            builder.CloseComponent();
            builder.CloseElement();
        }

        if (ShowDots && items.Count > 1)
        {
            builder.OpenElement(25, "div");
            builder.AddAttribute(26, "class", "fa-carousel-dots");
            var seq = 27;
            for (var i = 0; i < items.Count; i++)
            {
                var dotIndex = i;
                builder.OpenElement(seq++, "button");
                builder.SetKey(i);
                builder.AddAttribute(seq++, "type", "button");
                builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-carousel-dot", i == _index ? "fa-carousel-dot-active" : null));
                builder.AddAttribute(seq++, "aria-label", $"Go to slide {i + 1}");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => GoTo(dotIndex)));
                builder.CloseElement();
            }
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
