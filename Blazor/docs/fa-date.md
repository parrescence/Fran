[← Back to index](index.md)

# FaDate

Split day/month/year date entry with a calendar popup, `Min`/`Max` range support, and
an optional floating label. `InputBase<DateOnly?>`-derived, so it only works inside
an `<EditForm>`. Almost no JS: the popup and "close on focus leaving the control" are
plain Blazor state with no JS interop at all. The one exception is the compact wheel
picker's scroll-to-select, which needs one small vanilla-JS file with no Blazor
interop — see [Compact scroll picker](#compact-scroll-picker) below.

## Usage

```razor
<EditForm Model="_model">
    <FaDate @bind-Value="_model.Date"
            Label="Start date"
            Max="DateOnly.FromDateTime(DateTime.Today)" />
</EditForm>

@code {
    private class ItemModel
    {
        public DateOnly? Date { get; set; }
    }

    private ItemModel _model = new();
}
```

## Getting the value

`@bind-Value` writes into `_model.Date` — typing all three fields, using the arrow
keys, or picking a day on the calendar all commit through the same path.

## Keyboard & scroll

Within the day/month/year fields, Up/Down spin the focused field ±1 (wrapping at its
min/max) and Left/Right hop focus to the adjacent field instead of moving the text
caret. Mouse-wheel/trackpad scroll over a field is the same ±1 spin as Up/Down; scroll
over the calendar popup's year stepper spins the displayed year without touching the
committed value.

## Small viewports

Below 720px wide the calendar popup switches to a sticky sheet pinned near the
viewport's bottom edge — at its own natural size, just centered and repositioned, so
nothing about it gets clipped or forced into an inner scrollbar — instead of the
normal popup anchored under the input, which can otherwise run off-screen on a
short/mobile viewport depending on where the field sits on the page. Still no JS: it's
a CSS-only override of the same `fa-date-calendar-open` class (see `_responsive.scss`).

## Compact scroll picker

The calendar header's second icon button (next to the toggle that opens/closes the
popup) swaps the day grid for three independently-scrollable day/month/year wheels —
a smaller, thumb-friendlier picker, each with a plain number input above it so a
value can be typed instead of scrolled to. A wheel value can be set three ways, all
going through the same commit path: type into the input above it, scroll it so the
wanted value settles in the highlighted middle band, or tap the value directly.
Typing directly into the day/month/year fields above the popup still works exactly
the same in either mode too — those fields aren't part of the popup, so switching
modes never loses whatever's already been typed.

Each column curves like a real spinning wheel rather than scrolling as a flat list —
rows tilt, shrink, and fade the further they sit from the highlighted band, with the
curve itself animating live as you scroll. Purely a visual effect layered on top of
plain scroll position; which value ends up selected is unaffected either way.

A wheel spins the same way for a mouse as it does for a finger — press and drag
(or flick) it up/down, released momentum decays at a capped, readable speed rather
than a fast/blurry fling. The Day and Month wheels also roll continuously: spinning
past 31 wraps back to 1 (and past 1 back to 31) in either direction, same for
December/January, so neither one ever "runs out" mid-spin. Year doesn't wrap — it
just keeps counting toward `Min`/`Max` like before.

`Min`/`Max` narrow all three wheels, not just which days the day grid greys out —
Month and Year lock out whichever of their own values can't produce a single valid
day given the other two, and Day locks out anything outside the range once Month/Year
already land inside it (a `Min`/`Max` a week apart, say, locks Day down to just that
week). A locked value still shows, dimmed, rather than disappearing — the wheel still
reads as one continuous thing to spin through, just with a stretch you can't land on.
Year keeps a buffer of locked years on either side of `Min`/`Max` for the same
reason (so the true edge of what's pickable never sits at the very top or bottom of
the wheel where centering it exactly isn't possible) even when that leaves only one
real year to pick. When `Min`/`Max` narrow things down to exactly one valid date,
`Value` defaults straight to it — there's nothing else it could be.

When `Min`/`Max` leave a wheel exactly one selectable value — Year almost always,
Month too whenever `Min`/`Max` fall in the same year — that wheel stops accepting
scroll/drag input entirely rather than letting it spin freely through a column of
values it can never land on, and the one value it's locked to is filled in for you
up front instead of waiting for a click that column can no longer receive. A
`Min`/`Max` spanning one whole month (`8/1/2026`–`8/31/2026`, say) locks Year and
Month to `2026`/`August` immediately and leaves Day the only wheel actually worth
scrolling — spin it, tap a value, or type into the number input above it, same as
always.

More than one but still fewer choices than the wheel renders — `Min`/`Max` a year or
two apart, say — stops short of a full lock (there's still something to scroll
between) but still stops the wheel exactly at those real choices rather than letting
it scroll on into the disabled padding around them: a `Min`/`Max` of `2026`–`2027`
only ever lets the Year wheel land on `2026` or `2027`, no matter how far or how many
times you scroll past either one.

Scroll-to-select (letting go once the wanted value is centered) needs
`wwwroot/js/fa-date-wheel.js` wired into your host page — see
[install.md](install.md). Without it, the wheels still scroll, the number inputs and
tapping a value still work; only "just let go" stops committing a value on its own.

Set `CompactPicker="true"` to lock the popup to the wheels — the header's mode toggle
button is hidden entirely, since there'd be nothing else for it to switch to:

```razor
<FaDate @bind-Value="_model.Date" Label="Pick a date" CompactPicker="true" />
```

### Auto-switching by screen width

Without `CompactPicker`, which one shows isn't fixed — a plain CSS media query at the
720px breakpoint this library treats as "small screen" everywhere else picks the grid
above it and the wheels at/under it, live, as the viewport crosses it (rotating a
tablet, resizing a browser window). No JavaScript is involved: both the grid and the
wheels are always rendered, and CSS alone decides which one paints. Clicking the
header's mode-toggle button overrides that for good on that one `FaDate` instance —
whichever mode you pick then stays fixed regardless of viewport width afterward,
exactly like `CompactPicker` does, just per-instance and undoable only by reloading.
Custom `Format` order/widths (see below) apply the same way to both variants either
way, so the wheels never fall out of sync with whatever order the grid or split
fields above the popup are using.

## Custom field order

`Format` sets both the field order and each field's width, using the same `y`/`M`/`d`
token letters as a .NET custom date format string (case-insensitive here, since
there's no time-of-day component to disambiguate `m` from minutes):

```razor
<FaDate @bind-Value="_model.Date" Label="Order placed" Format="yyyy-MM-dd" />
```

A run's length sets that field's display width — `yyyy` is a full 4-digit year, `yy`
stores/shows only the last two digits (resolved back to a full year through a
+/-50-year pivot around today); `MM`/`dd` zero-pad to 2 digits, `M`/`d` don't.
Anything that isn't `y`/`M`/`d` is a literal separator rendered as-is — not limited to
`/`, so `dd.MM.yyyy` or `yyyy MM dd` both work. Defaults to `"MM/dd/yyyy"`; a format
missing exactly one of each field falls back to that default rather than rendering a
control that can't represent a full date.

## Min/Max validation

If both `Min` and `Max` are set and `Max` ends up before `Min`, FaDate shows a
`.fa-validation-message` under the label ("Max (…) must be on or after Min (…).")
instead of silently making every day unpickable. This is FaDate checking its own
two parameters directly — `Min`/`Max` aren't bound model fields, so this doesn't
go through [FaFa's validation system](validation.md) at all. It's shown, not
thrown: a transient bad combination (Min/Max still loading from data, say) doesn't
crash the render tree, and the rest of the picker stays interactive.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Value` / `ValueChanged` | `DateOnly?` | `@bind-Value` |
| `Label` | `string?` | |
| `FloatingLabel` | `bool` | label overlaps the field instead of sitting above it |
| `Min` / `Max` | `DateOnly?` | |
| `Format` | `string` | field order + width, e.g. `"yyyy-MM-dd"`; defaults to `"MM/dd/yyyy"` |
| `ReadOnly` | `bool` | flattens the day/month/year fields, toggle, and popup to the formatted date as plain text with a bottom border |
| `CompactPicker` | `bool` | locks the popup to the compact day/month/year wheels; hides the header's mode toggle entirely |
| `ContainerCssClass` | `string?` | |

[← Back to index](index.md)
