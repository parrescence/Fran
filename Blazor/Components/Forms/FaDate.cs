using System.Diagnostics.CodeAnalysis;
using FaFa.Icons;
using FaFa.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace FaFa.Components;

/// <summary>
/// Split day/month/year date entry with a calendar popup, min/max range support, and
/// an optional floating label. A clean rewrite of an older library's date picker: the
/// original idea (three separately-editable, arrow-key-navigable number fields plus a
/// calendar grid) is worth keeping, but this version has no reflection, no ids
/// regenerated on every render, and almost no JS — the popup's open/closed state is
/// plain Blazor state, and "close when focus leaves the control" is a native
/// <c>@onfocusout</c>-with-a-grace-period pattern (<see cref="HandleFocusOutAsync"/>)
/// instead of a document click listener reaching back into Blazor over JS interop.
/// Each day/month/year field (and the calendar popup's year stepper) also accepts
/// mouse-wheel/trackpad scroll as a spin gesture, the same +1/-1 step as the Up/Down
/// arrow keys — see <see cref="HandleFieldWheel"/>/<see cref="HandleDisplayYearWheel"/>.
/// Left/Right on a field hops focus to the adjacent field (<see cref="FocusAdjacentField"/>)
/// instead of moving the text caret — with each field auto-advancing forward on its
/// own once full, there's no in-field caret position worth preserving.
/// <see cref="Format"/> controls both the field order and each field's width — e.g.
/// <c>"yyyy-MM-dd"</c>, <c>"M/d/yy"</c>, <c>"dd.MM.yyyy"</c> — see
/// <see cref="ParseFormat"/> for the token rules. The one exception to "no JS" is the
/// compact wheel picker's scroll-to-select (<see cref="RenderWheels"/>): knowing which
/// item a user scrolled to rest on needs real scroll-position math (element positions
/// relative to the container's center) that a Blazor scroll event's args don't carry,
/// so <c>wwwroot/js/fa-date-wheel.js</c> — a small vanilla-JS, no-Blazor-interop file,
/// same idiom as <c>theme.js</c>/<c>sidebar.js</c> — watches each wheel, and once
/// scrolling settles, calls <c>.click()</c> on whichever item ended up centered. That
/// reuses the exact same commit path a manual click already goes through
/// (<see cref="SelectWheelValue"/>) rather than reaching back into Blazor itself. The
/// same file also handles the opposite direction — scrolling a wheel to match a value
/// set some other way (typed into the number input above it, or the popup just
/// opened) — via a <c>MutationObserver</c> watching for which item currently carries
/// <c>fa-date-wheel-item-selected</c> and calling <c>.scrollIntoView()</c> on it. That
/// used to be done from C# with <c>ElementReference.FocusAsync()</c> (scrolling into
/// view is a side effect of focusing), but focusing a wheel button also moves real
/// keyboard focus — doing that after every keystroke kept yanking focus off the
/// input the user was actively typing into. <c>scrollIntoView</c> only ever changes
/// scroll position, never focus, so there's nothing left in this component watching
/// for "what changed, and does a wheel need re-centering" at all.
/// </summary>
public sealed class FaDate : InputBase<DateOnly?>
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool FloatingLabel { get; set; }
    [Parameter] public DateOnly? Min { get; set; }
    [Parameter] public DateOnly? Max { get; set; }
    [Parameter] public string? ContainerCssClass { get; set; }
    /// <summary>
    /// Flattens the day/month/year fields, calendar toggle, and popup down to the
    /// formatted date as plain text with a bottom border — the shared "other fa
    /// styles" readonly look (see FaInput/FaSelect/etc. for the boxed-muted style
    /// used by native inputs).
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Locks the calendar popup to the compact day/month/year wheel picker (see
    /// <see cref="RenderWheels"/>) instead of the day grid — for a consumer who
    /// only ever wants the smaller picker, not a default the end user can still
    /// switch away from. The header's mode toggle button is hidden entirely
    /// while this is set, since there'd be nothing else to switch to.
    /// </summary>
    [Parameter] public bool CompactPicker { get; set; }

    /// <summary>
    /// Field order and per-field width, using the same <c>y</c>/<c>M</c>/<c>d</c>
    /// token letters as a .NET custom date format string (case-insensitive here,
    /// since there's no time-of-day component to disambiguate <c>m</c> from
    /// minutes). A run's length sets that field's display width — <c>yyyy</c> is a
    /// full 4-digit year, <c>yy</c> stores/shows only the last two digits (resolved
    /// back to a full year through a +/-50-year pivot around today, see
    /// <see cref="ResolveTwoDigitYear"/>); <c>MM</c>/<c>dd</c> zero-pad to 2 digits,
    /// <c>M</c>/<c>d</c> don't. Anything that isn't <c>y</c>/<c>M</c>/<c>d</c> is a
    /// literal separator rendered as-is (not limited to <c>/</c>). Defaults to
    /// <c>"MM/dd/yyyy"</c>; a format missing exactly one of each field falls back to
    /// that default rather than rendering a control that can't represent a full date.
    /// </summary>
    [Parameter] public string Format { get; set; } = "MM/dd/yyyy";

    private enum DateField { Day, Month, Year }

    private readonly record struct FormatSegment(DateField? Field, int Width, string? Literal);

    private readonly string _wrapperId = $"fa-date-{Guid.NewGuid():N}";
    private readonly string _dayId = $"fa-date-day-{Guid.NewGuid():N}";
    private readonly string _monthId = $"fa-date-month-{Guid.NewGuid():N}";
    private readonly string _yearId = $"fa-date-year-{Guid.NewGuid():N}";

    private ElementReference _dayRef;
    private ElementReference _monthRef;
    private ElementReference _yearRef;

    // The grid and wheels headers' own mode-toggle buttons (see
    // RenderCalendarHeader/RenderPickerModeToggle) — captured so SetPickerMode
    // can refocus whichever one just became visible. Needed because a manual
    // toggle click doesn't destroy/recreate either button (see RenderCalendar's
    // own remarks on why that matters) — it hides the *other* variant's whole
    // wrapper via a CSS class instead. But a display:none'd element can't hold
    // focus either: the browser force-blurs it to <body> the instant its
    // wrapper disappears, which — with nothing to catch it — reads to
    // HandleFocusOutAsync as focus leaving the control entirely and closes the
    // popup right back out from under the click that was supposed to switch
    // modes. Refocusing the new (still-visible) toggle in OnAfterRenderAsync,
    // once the DOM's actually caught up with the class change, keeps focus
    // inside the wrapper the whole time instead.
    private ElementReference _desktopModeToggleRef;
    private ElementReference _mobileModeToggleRef;
    private bool _focusModeToggleAfterRender;

    private string _dayText = "";
    private string _monthText = "";
    private string _yearText = "";

    private string? _parsedFormat;
    private List<FormatSegment> _segments = [];
    private List<DateField> _fieldOrder = [];
    private int _yearWidth = 4;
    private int _monthWidth = 2;
    private int _dayWidth = 2;

    private bool _isOpen;
    private int _displayYear;
    private int _displayMonth;
    private bool _displayInitialized;
    private CancellationTokenSource? _pendingClose;

    // Alternate popup body for small screens: three scrollable day/month/year
    // "wheel" lists instead of the day grid — a smaller, thumb-friendly target.
    // _compactPicker is the end user's own toggle state (header button, only
    // rendered when CompactPicker isn't already forcing it — see EffectiveCompact);
    // typing directly into the day/month/year fields above the popup keeps
    // working unchanged in either mode, since those fields aren't part of the
    // popup at all.
    private bool _compactPicker;
    private bool EffectiveCompact => CompactPicker || _compactPicker;

    // Until the consumer forces a mode (CompactPicker) or the person using it
    // clicks the header's own toggle once (SetPickerMode), which mode shows
    // isn't decided here at all — RenderCalendar renders *both* the grid and
    // the wheels, and a CSS media query at the same 720px breakpoint
    // _responsive.scss uses everywhere else picks whichever one actually
    // shows, live, as the viewport crosses it. Plain CSS rather than a JS
    // matchMedia listener + interop callback — no window-size hook exists in
    // Blazor itself to react to, but "which of two already-rendered things
    // is visible" is exactly what CSS already does on its own, so nothing
    // here needed to reach past it.
    private bool _pickerModeManuallySet;

    private readonly string _wheelDayInputId = $"fa-date-wheel-day-{Guid.NewGuid():N}";
    private readonly string _wheelMonthInputId = $"fa-date-wheel-month-{Guid.NewGuid():N}";
    private readonly string _wheelYearInputId = $"fa-date-wheel-year-{Guid.NewGuid():N}";

    protected override bool TryParseValueFromString(string? value, out DateOnly? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (DateOnly.TryParse(value, out var parsed))
        {
            result = parsed;
            validationErrorMessage = null;
            return true;
        }

        result = null;
        validationErrorMessage = $"'{value}' is not a valid date.";
        return false;
    }

    protected override void OnParametersSet()
    {
        EnsureFormatParsed();
        SyncTextFromValue();

        if (!_displayInitialized)
        {
            var basis = Value ?? DateOnly.FromDateTime(DateTime.Today);
            _displayYear = basis.Year;
            _displayMonth = basis.Month;
            _displayInitialized = true;
        }
    }

    private void EnsureFormatParsed()
    {
        var format = string.IsNullOrWhiteSpace(Format) ? "MM/dd/yyyy" : Format;
        if (format == _parsedFormat)
        {
            return;
        }

        var segments = ParseFormat(format);
        var fields = segments.Where(s => s.Field is not null).Select(s => s.Field!.Value).ToList();

        if (fields.Count != 3 || fields.Distinct().Count() != 3)
        {
            format = "MM/dd/yyyy";
            segments = ParseFormat(format);
            fields = segments.Where(s => s.Field is not null).Select(s => s.Field!.Value).ToList();
        }

        _segments = segments;
        _fieldOrder = fields;
        _yearWidth = segments.First(s => s.Field == DateField.Year).Width;
        _monthWidth = segments.First(s => s.Field == DateField.Month).Width;
        _dayWidth = segments.First(s => s.Field == DateField.Day).Width;
        _parsedFormat = format;
    }

    // Splits a format string into field runs (consecutive y/M/d, case-insensitive)
    // and literal runs (everything else, kept verbatim as typed — a separator isn't
    // limited to a single "/" character).
    private static List<FormatSegment> ParseFormat(string format)
    {
        var segments = new List<FormatSegment>();
        var i = 0;
        while (i < format.Length)
        {
            var lower = char.ToLowerInvariant(format[i]);
            if (lower is 'y' or 'm' or 'd')
            {
                var j = i;
                while (j < format.Length && char.ToLowerInvariant(format[j]) == lower)
                {
                    j++;
                }

                var field = lower switch { 'y' => DateField.Year, 'm' => DateField.Month, _ => DateField.Day };
                segments.Add(new FormatSegment(field, j - i, null));
                i = j;
            }
            else
            {
                var j = i;
                while (j < format.Length && char.ToLowerInvariant(format[j]) is not ('y' or 'm' or 'd'))
                {
                    j++;
                }

                segments.Add(new FormatSegment(null, 0, format[i..j]));
                i = j;
            }
        }

        return segments;
    }

    private string GetText(DateField field) => field switch
    {
        DateField.Month => _monthText,
        DateField.Day => _dayText,
        _ => _yearText
    };

    private void SetText(DateField field, string value)
    {
        switch (field)
        {
            case DateField.Month: _monthText = value; break;
            case DateField.Day: _dayText = value; break;
            default: _yearText = value; break;
        }
    }

    private string IdFor(DateField field) => field switch
    {
        DateField.Month => _monthId,
        DateField.Day => _dayId,
        _ => _yearId
    };

    private void SetRef(DateField field, ElementReference reference)
    {
        switch (field)
        {
            case DateField.Month: _monthRef = reference; break;
            case DateField.Day: _dayRef = reference; break;
            default: _yearRef = reference; break;
        }
    }

    private int Width(DateField field) => field switch
    {
        DateField.Month => _monthWidth,
        DateField.Day => _dayWidth,
        _ => _yearWidth
    };

    private string Placeholder(DateField field)
    {
        var letter = field switch { DateField.Month => 'm', DateField.Day => 'd', _ => 'y' };
        return new string(letter, Width(field));
    }

    // Typed digit cap: month/day never need more than 2 digits regardless of
    // whether Format asked for the zero-padded ("MM") or bare ("M") display width.
    // A "yy" year only ever holds 2 typed digits; anything wider (including the
    // default "yyyy") allows a full 4-digit year.
    private int MaxLength(DateField field) => field == DateField.Year ? (_yearWidth <= 2 ? 2 : 4) : 2;

    private bool AllFieldsEmpty => string.IsNullOrEmpty(_dayText) && string.IsNullOrEmpty(_monthText) && string.IsNullOrEmpty(_yearText);

    private void SyncTextFromValue()
    {
        if (Value is { } date)
        {
            _dayText = FormatFieldValue(DateField.Day, date.Day);
            _monthText = FormatFieldValue(DateField.Month, date.Month);
            _yearText = FormatFieldValue(DateField.Year, date.Year);
        }
        else
        {
            _dayText = "";
            _monthText = "";
            _yearText = "";
        }
    }

    private string FormatFieldValue(DateField field, int value)
    {
        if (field == DateField.Year && _yearWidth <= 2)
        {
            return (value % 100).ToString("D2");
        }

        return value.ToString().PadLeft(Width(field), '0');
    }

    private bool WithinRange(DateOnly date) => (Min is null || date >= Min) && (Max is null || date <= Max);

    // advanceFocus is false for the wheel-column number inputs (RenderWheelColumn) —
    // those live inside the popup, so auto-advancing into _monthRef/_dayRef/_yearRef
    // (the split fields above the popup, the only thing FocusAdjacentField knows how
    // to reach) would yank focus out of the popup the user is actively working in.
    private void HandleFieldInput(DateField field, string? rawValue, bool advanceFocus = true)
    {
        var digits = new string((rawValue ?? "").Where(char.IsDigit).ToArray());
        var maxLength = MaxLength(field);
        if (digits.Length > maxLength)
        {
            digits = digits[..maxLength];
        }

        // Only updates local text state here — committing (CurrentValue, which
        // round-trips through the bound Value parameter and can reformat/zero-pad
        // whatever's in a field) happens on blur instead (see HandleFieldBlur),
        // not on every keystroke. Committing mid-typing meant a field that
        // happened to already parse as "valid" after a single leading digit
        // (year "1" already satisfies >= 1, month "1" already satisfies <= 12)
        // got zero-padded back into itself right away ("1" -> "01"/"0001"),
        // filling its own maxlength before the rest of the digits were typed —
        // "the year field won't let me type a full year" was this repeatedly
        // guessing at an incomplete value instead of waiting to be asked.
        SetText(field, digits);

        // Typing into the split fields above the popup keeps the compact wheel
        // picker in step too, whenever the wheels might actually be the thing
        // showing — RenderWheels reads _dayText/_monthText/_yearText directly,
        // so the right item highlights (and, via fa-date-wheel.js's
        // MutationObserver on that highlight class, scrolls into view) with no
        // extra work here beyond keeping _displayMonth/_displayYear in step on
        // a completed month/year, same as HandleWheelFieldInput does for its
        // own input row, so the day wheel's count and the calendar header stay
        // consistent either way the user typed. "Might actually be showing" is
        // EffectiveCompact (CompactPicker, or a manual toggle onto wheels) OR
        // simply not yet manually locked onto grid at all — in that auto state
        // RenderCalendar renders *both* variants and lets the viewport's own
        // CSS decide which one's visible, so this can't tell from C# state
        // alone whether the wheels are on screen right now. Keeping their state
        // in sync regardless is harmless even while they're the hidden one.
        if (_isOpen && (EffectiveCompact || !_pickerModeManuallySet))
        {
            if (field == DateField.Month && digits.Length == maxLength && int.TryParse(digits, out var typedMonth) && typedMonth is >= 1 and <= 12)
            {
                _displayMonth = typedMonth;
            }
            else if (field == DateField.Year && digits.Length == maxLength && int.TryParse(digits, out var typedYear))
            {
                _displayYear = ResolveYear(typedYear);
            }
        }

        // Auto-advance to the next field (in whatever order Format put it) once this
        // one is full, same idea as a native multi-part date input.
        if (advanceFocus && digits.Length == maxLength)
        {
            _ = FocusAdjacentField(field, 1);
        }
    }

    // The wheel-column number input's own oninput — same digit parsing/commit as
    // HandleFieldInput, minus the auto-advance-out-of-the-popup behavior, plus
    // clamping a completed value into range. The wheel itself re-centers on
    // whatever ends up selected without any help from here — RenderWheels reads
    // _monthText/_dayText/_yearText directly every render, and fa-date-wheel.js's
    // MutationObserver scrolls whichever item that highlights into view.
    private void HandleWheelFieldInput(DateField field, string? rawValue)
    {
        HandleFieldInput(field, rawValue, advanceFocus: false);

        // Once a full-width number's been typed, clamp it into the field's valid
        // range and redisplay zero-padded — the same shape a wheel click already
        // commits via SelectWheelValue/FormatFieldValue (typing "1" for January
        // should end up "01", same as scrolling to it) — instead of leaving a raw,
        // possibly out-of-range value (month "13", day "45") sitting in the input.
        // An out-of-range month is exactly what used to reach AdjustField's Day
        // case unclamped and throw straight out of DateTime.DaysInMonth.
        if (GetText(field).Length == MaxLength(field) && int.TryParse(GetText(field), out var typed))
        {
            var clamped = field switch
            {
                DateField.Month => Math.Clamp(typed, 1, 12),
                DateField.Day => Math.Clamp(typed, 1, CurrentWheelBasis().DaysInMonth),
                _ => typed
            };
            SetText(field, FormatFieldValue(field, clamped));

            if (field == DateField.Month)
            {
                _displayMonth = clamped;
            }
            else if (field == DateField.Year)
            {
                _displayYear = clamped;
            }

            // Committing here too would fight the same premature-commit problem
            // HandleFieldInput's own remarks describe — this field being complete
            // doesn't mean the other two are, and there's no reason to guess with
            // whatever's currently sitting in them. See HandleFieldBlur.
        }
    }

    // The split fields' and wheel-input row's shared onblur — this is the only
    // place typed digits actually commit to CurrentValue (see HandleFieldInput's
    // remarks for why oninput itself no longer does). Fires once per field, once
    // the user's done with it, using whatever's in all three fields at that
    // point — TryCommitValue's own length guard below still leaves an
    // incomplete combination alone rather than guessing at it.
    private void HandleFieldBlur() => TryCommitValue();

    private void TryCommitValue()
    {
        if (AllFieldsEmpty)
        {
            CurrentValue = null;
            return;
        }

        // Belt-and-suspenders alongside HandleFieldBlur only calling this once
        // typing's done: still refuse to guess at a field that's shorter than
        // its expected length (e.g. a blur mid-entry, or a future caller on the
        // oninput path). A 2-digit-width field (the default "MM"/"dd"/"yyyy")
        // already reads as a "valid" value the moment a single leading digit
        // happens to parse to one — month "1" and year "1" both already satisfy
        // month is >=1 and <=12 / year >= 1 below — and committing that
        // zero-pads it right back into the field ("1" -> "01", "1" -> "0001"),
        // filling its own maxlength before the rest of the digits exist. Using
        // Format's own declared width for day/month (so a genuinely
        // single-digit "M"/"d" format still commits at one digit, since
        // MaxLength there stays wider than Width only to *allow* an optional
        // second one) and MaxLength for year (year has no narrow-format
        // equivalent — it's either fully 2 or fully 4 digits) tells a real
        // partial value apart from an intentionally short one.
        if (_dayText.Length < Width(DateField.Day) || _monthText.Length < Width(DateField.Month) || _yearText.Length < MaxLength(DateField.Year))
        {
            return;
        }

        if (int.TryParse(_dayText, out var day) && int.TryParse(_monthText, out var month) &&
            int.TryParse(_yearText, out var yearDigits))
        {
            var year = ResolveYear(yearDigits);
            if (year >= 1 && month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month))
            {
                var candidate = new DateOnly(year, month, day);
                if (WithinRange(candidate))
                {
                    CurrentValue = candidate;
                    _displayYear = year;
                    _displayMonth = month;
                }
            }
        }

        // Incomplete or currently out-of-range digits: leave CurrentValue where it is
        // and let the user keep typing rather than fighting them mid-entry.
    }

    // A "yy" format only ever stores the last two typed digits — expand them back to
    // a full year around today's, the same +/-50-year pivot window most short-date
    // pickers use, rather than always assuming 19xx or 20xx.
    private static int ResolveTwoDigitYear(int twoDigit)
    {
        var today = DateTime.Today.Year;
        var candidate = today / 100 * 100 + twoDigit;
        if (candidate > today + 50)
        {
            candidate -= 100;
        }
        else if (candidate < today - 50)
        {
            candidate += 100;
        }

        return candidate;
    }

    private int ResolveYear(int yearDigits) => _yearWidth <= 2 ? ResolveTwoDigitYear(yearDigits) : yearDigits;

    // Shared "what month/year is the compact picker currently basing its day count
    // on" logic — used both to size the day wheel/grid (RenderWheels) and to clamp a
    // freshly-typed day value into range (HandleWheelFieldInput). An out-of-range or
    // unparsable month/year falls back to whatever the calendar's already showing
    // rather than feeding DateTime.DaysInMonth a value it'll throw on.
    private (int Month, int Year, int DaysInMonth) CurrentWheelBasis()
    {
        var month = int.TryParse(_monthText, out var m) && m is >= 1 and <= 12 ? m : _displayMonth;
        var year = int.TryParse(_yearText, out var y) ? ResolveYear(y) : _displayYear;
        return (month, year, DateTime.DaysInMonth(Math.Max(1, year), month));
    }

    private void AdjustField(DateField field, int delta)
    {
        switch (field)
        {
            case DateField.Year:
            {
                var current = int.TryParse(_yearText, out var y) ? y : _yearWidth <= 2 ? DateTime.Today.Year % 100 : DateTime.Today.Year;
                var next = current + delta;
                next = _yearWidth <= 2 ? ((next % 100) + 100) % 100 : Math.Max(1, next);
                _yearText = FormatFieldValue(DateField.Year, next);
                break;
            }
            case DateField.Month:
            {
                var month = (int.TryParse(_monthText, out var m) ? m : DateTime.Today.Month) + delta;
                month = ((month - 1 + 12) % 12) + 1;
                _monthText = FormatFieldValue(DateField.Month, month);
                break;
            }
            case DateField.Day:
            {
                // _monthText/_yearText can hold a value typed only partway to valid —
                // an out-of-range month (e.g. "13" mid-typing "1" then "3") or a "0000"
                // year — while the day field is still spun with arrow keys/wheel. Both
                // get clamped into DateTime.DaysInMonth's valid 1-12/1-9999 ranges rather
                // than passed through raw, which used to throw ArgumentOutOfRangeException
                // straight out of DaysInMonth and crash the picker on the next Day spin.
                var maxDay = int.TryParse(_yearText, out var yy) && int.TryParse(_monthText, out var mm)
                    ? DateTime.DaysInMonth(Math.Max(1, ResolveYear(yy)), Math.Clamp(mm, 1, 12))
                    : 31;
                var day = (int.TryParse(_dayText, out var d) ? d : DateTime.Today.Day) + delta;
                day = ((day - 1 + maxDay) % maxDay) + 1;
                _dayText = FormatFieldValue(DateField.Day, day);
                break;
            }
        }

        TryCommitValue();
    }

    private void HandleFieldKeyDown(KeyboardEventArgs e, DateField field)
    {
        switch (e.Key)
        {
            case "ArrowUp": AdjustField(field, 1); break;
            case "ArrowDown": AdjustField(field, -1); break;
            // Left/Right hop to the adjacent field in whatever order Format put it,
            // instead of the browser's own default of moving the text caret — with
            // each field auto-advancing forward on its own once full (see
            // HandleFieldInput), there's no in-field caret position worth preserving.
            case "ArrowLeft": _ = FocusAdjacentField(field, -1); break;
            case "ArrowRight": _ = FocusAdjacentField(field, 1); break;
        }
    }

    private ValueTask FocusAdjacentField(DateField field, int delta)
    {
        var index = _fieldOrder.IndexOf(field) + delta;
        return index >= 0 && index < _fieldOrder.Count ? FocusField(_fieldOrder[index]) : ValueTask.CompletedTask;
    }

    private ValueTask FocusField(DateField field) => field switch
    {
        DateField.Month => _monthRef.FocusAsync(),
        DateField.Day => _dayRef.FocusAsync(),
        DateField.Year => _yearRef.FocusAsync(),
        _ => ValueTask.CompletedTask
    };

    // Mouse-wheel spin, same +1/-1 step as the arrow keys above — lets a mouse (or
    // trackpad scroll) drive the day/month/year fields the way the calendar's own
    // month nav already scrolls, instead of typing being the only fast path.
    // PreventDefault (below, on the "onwheel" attribute) stops the page itself from
    // scrolling while the pointer is sitting over the field.
    private void HandleFieldWheel(DateField field, WheelEventArgs e) => AdjustField(field, e.DeltaY < 0 ? 1 : -1);

    // Same wheel-to-spin idea, but for the calendar popup's own year stepper — moves
    // _displayYear (which month/year the grid shows), not the split fields' committed
    // value, exactly like typing into that stepper's onchange already does.
    private void HandleDisplayYearWheel(WheelEventArgs e) => _displayYear = Math.Max(1, _displayYear + (e.DeltaY < 0 ? 1 : -1));

    private void SelectDay(int day)
    {
        var candidate = new DateOnly(_displayYear, _displayMonth, day);
        if (!WithinRange(candidate))
        {
            return;
        }

        CurrentValue = candidate;
        SyncTextFromValue();
        _isOpen = false;
    }

    private void GoToToday()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        _displayYear = today.Year;
        _displayMonth = today.Month;
        if (WithinRange(today))
        {
            CurrentValue = today;
            SyncTextFromValue();
        }
    }

    private void Clear()
    {
        CurrentValue = null;
        SyncTextFromValue();
    }

    private void ChangeDisplayMonth(int delta)
    {
        var total = (_displayYear * 12 + (_displayMonth - 1)) + delta;
        _displayYear = total / 12;
        _displayMonth = (total % 12) + 1;
    }

    // Opening straight into compact mode (CompactPicker="true", or the user left
    // the header toggle on compact from a previous open) doesn't need any extra
    // centering step here — the wheels stay mounted in the DOM even while
    // closed (just hidden via the "fa-date-calendar-open" class below), so
    // fa-date-wheel.js's MutationObserver treats that class landing as its own
    // cue to re-center whatever's already selected.
    private void ToggleOpen()
    {
        CancelPendingClose();
        _isOpen = !_isOpen;
    }

    private void HandleFocusIn() => CancelPendingClose();

    // No document click listener / JS interop needed to close the popup when focus
    // leaves the control: focusout already fires when focus moves anywhere outside
    // this wrapper (including outside the browser). A short grace period just lets a
    // focusin on a sibling field inside the same wrapper (e.g. tabbing from the day
    // input to the month input) cancel the pending close instead of flickering shut.
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

    private void HandleWrapperKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            _isOpen = false;
        }
    }

    // Only reachable when CompactPicker isn't already forcing compact mode on —
    // RenderPickerModeToggle isn't even rendered in that case (see RenderCalendar).
    // Switching into compact mode inserts the wheels' DOM nodes for the first
    // time on a component that hasn't shown them yet (they don't exist at all
    // in grid mode), which is itself a childList mutation fa-date-wheel.js's
    // MutationObserver already reacts to — nothing extra needed here to get
    // them centered.
    //
    // Also the one place _pickerModeManuallySet gets set — a deliberate click
    // here means "stop following the viewport and just show what I picked",
    // permanently opting this instance out of the auto (screen-width-driven)
    // behavior RenderCalendar falls back to otherwise for the rest of its
    // lifetime. Takes the target mode explicitly rather than negating
    // _compactPicker — in auto mode _compactPicker doesn't necessarily match
    // which variant the user is actually looking at (that's decided by CSS,
    // not this field), so each variant's own toggle button passes its own
    // `compact` straight through instead (see RenderCalendarHeader).
    private void SetPickerMode(bool compact)
    {
        _compactPicker = compact;
        _pickerModeManuallySet = true;

        // Picked up by OnAfterRenderAsync once this render's actually landed —
        // see _desktopModeToggleRef's own remarks for why the click that
        // fires this needs a deliberate refocus at all.
        _focusModeToggleAfterRender = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_focusModeToggleAfterRender)
        {
            return;
        }

        _focusModeToggleAfterRender = false;
        var target = _compactPicker ? _mobileModeToggleRef : _desktopModeToggleRef;
        try
        {
            await target.FocusAsync();
        }
        catch (JSException)
        {
            // The toggle isn't rendered at all when CompactPicker forces the
            // mode outright — SetPickerMode is unreachable there (its own
            // button is never rendered to click in the first place), so this
            // is just a defensive catch, not an expected path.
        }
    }

    // A wheel item's onclick, same commit path typing a field already goes
    // through (SetText + TryCommitValue) — the wheels are just another way to
    // fill in the same three text fields, not a parallel source of truth.
    private void SelectWheelValue(DateField field, int value)
    {
        SetText(field, FormatFieldValue(field, value));

        // Keep the grid (and the other wheel columns, which read _displayYear/
        // _displayMonth for the day column's day-count) in step immediately —
        // TryCommitValue only updates those once all three fields parse to a
        // complete, in-range date, which isn't true yet mid-selection.
        if (field == DateField.Month)
        {
            _displayMonth = value;
        }
        else if (field == DateField.Year)
        {
            _displayYear = value;
        }

        TryCommitValue();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var hasLabel = !string.IsNullOrEmpty(Label);
        var seq = 0;

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-field", ContainerCssClass));

        if (hasLabel && !FloatingLabel)
        {
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "class", "fa-label");
            builder.AddContent(seq++, Label);
            builder.CloseElement();
        }

        if (ReadOnly)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "fa-readonly-flat");
            builder.AddContent(seq++, Value?.ToString("MMMM d, yyyy") ?? "");
            builder.CloseElement();
            builder.CloseElement();
            return;
        }

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "id", _wrapperId);
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-date", FloatingLabel ? "fa-date-floating" : null, Value.HasValue ? "fa-date-has-value" : null));
        builder.AddAttribute(seq++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleWrapperKeyDown));
        builder.AddAttribute(seq++, "onfocusin", EventCallback.Factory.Create(this, HandleFocusIn));
        builder.AddAttribute(seq++, "onfocusout", EventCallback.Factory.Create(this, HandleFocusOutAsync));

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-date-fields");

        // Field order and separator text both come from the parsed Format — see
        // EnsureFormatParsed/ParseFormat — rather than a fixed month/day/year layout.
        foreach (var segment in _segments)
        {
            seq = segment.Field is { } field
                ? RenderField(builder, seq, field, IdFor(field), Placeholder(field), MaxLength(field), GetText(field), r => SetRef(field, r))
                : RenderSeparator(builder, seq, segment.Literal ?? "");
        }

        builder.CloseElement();

        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-date-toggle");
        builder.AddAttribute(seq++, "aria-label", _isOpen ? "Close calendar" : "Open calendar");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, ToggleOpen));
        builder.OpenComponent<FaIcon>(seq++);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Name), FaIconName.Calendar);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Color), FaIconColor.Black);
        builder.AddComponentParameter(seq++, nameof(FaIcon.Size), 16);
        builder.CloseComponent();
        builder.CloseElement();

        if (hasLabel && FloatingLabel)
        {
            builder.OpenElement(seq++, "label");
            builder.AddAttribute(seq++, "class", "fa-label fa-date-floating-label");
            builder.AddContent(seq++, Label);
            builder.CloseElement();
        }

        RenderCalendar(builder, seq);

        builder.CloseElement();
        builder.CloseElement();
    }

    private int RenderSeparator(RenderTreeBuilder builder, int sequence, string text)
    {
        builder.OpenElement(sequence++, "span");
        builder.AddAttribute(sequence++, "class", "fa-date-separator");
        builder.AddAttribute(sequence++, "aria-hidden", "true");
        builder.AddContent(sequence++, text);
        builder.CloseElement();
        return sequence;
    }

    private int RenderField(RenderTreeBuilder builder, int sequence, DateField field, string id, string placeholder, int maxLength, string value, Action<ElementReference> captureRef)
    {
        builder.OpenElement(sequence++, "input");
        builder.AddAttribute(sequence++, "id", id);
        builder.AddAttribute(sequence++, "class", "fa-date-input");
        builder.AddAttribute(sequence++, "type", "text");
        builder.AddAttribute(sequence++, "inputmode", "numeric");
        builder.AddAttribute(sequence++, "placeholder", placeholder);
        builder.AddAttribute(sequence++, "maxlength", maxLength);
        builder.AddAttribute(sequence++, "value", value);
        builder.AddAttribute(sequence++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e => HandleFieldInput(field, e.Value?.ToString())));
        builder.AddAttribute(sequence++, "onblur", EventCallback.Factory.Create(this, HandleFieldBlur));
        builder.AddAttribute(sequence++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, e => HandleFieldKeyDown(e, field)));
        builder.AddAttribute(sequence++, "onwheel", EventCallback.Factory.Create<WheelEventArgs>(this, e => HandleFieldWheel(field, e)));
        builder.AddEventPreventDefaultAttribute(sequence++, "onwheel", true);
        builder.AddElementReferenceCapture(sequence++, captureRef);
        builder.CloseElement();
        return sequence;
    }

    private void RenderCalendar(RenderTreeBuilder builder, int sequence)
    {
        // Every sub-render below advances a single running `seq` rather than reusing
        // fixed "sequence + N" offsets — the header used to hand RenderMonthStepButton
        // a base offset that overran into the very next element's own fixed offset
        // (its 9 frames reached sequence+12, but the month/year span right after it
        // was still hardcoded to start at sequence+10). Sequence numbers going
        // backwards like that is exactly what Blazor's diff algorithm reads as "a new
        // loop started here", and on every render after the first (i.e. any Prev/Next
        // month click or year change) that misread which sibling was which sometimes
        // starting immediately — a day button would get diffed as if it were still
        // last render's already-fully-attributed node and skip having its own
        // type/class/disabled/onclick attributes applied at all, rendering as a bare
        // browser-default <button>N</button>. A single incrementing counter (already
        // used everywhere else in this file — BuildRenderTree, RenderField,
        // RenderSeparator) can't overlap like that.
        var seq = sequence;

        // CompactPicker locks the mode outright — nothing here needs to react to
        // the viewport at all, so this stays the plain single-variant render this
        // method always did before auto mode existed.
        if (CompactPicker)
        {
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-date-calendar", _isOpen ? "fa-date-calendar-open" : null, "fa-date-calendar-compact"));
            seq = RenderCalendarHeader(builder, seq, compact: true);
            seq = RenderCalendarBody(builder, seq, compact: true);
            seq = RenderCalendarFooter(builder, seq);
            builder.CloseElement();
            return;
        }

        // Otherwise both the grid and the wheels render, every time, regardless
        // of _pickerModeManuallySet — which one actually shows is entirely a CSS
        // question (a plain media query by default, or one of the
        // fa-date-calendar-force-* classes below once the header's own
        // mode-toggle button has been clicked), never a C# one. That's
        // deliberate: the render tree's *shape* here never depends on the
        // viewport or on _compactPicker, so switching between them — whether
        // the viewport crosses 720px or the toggle gets clicked — only ever
        // changes a class on this wrapper, not which elements exist. Anything
        // else (conditionally rendering only one variant, the way this method
        // used to before auto mode existed) means the toggle button itself
        // gets torn down and recreated by the very click that fired it, which
        // — per this method's own history, see the sequence-number remarks
        // above — reads as focus leaving the control entirely and closes the
        // popup right out from under the click that was supposed to switch
        // modes instead.
        var forceClass = _pickerModeManuallySet
            ? (_compactPicker ? "fa-date-calendar-force-compact" : "fa-date-calendar-force-grid")
            : null;

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-date-calendar", _isOpen ? "fa-date-calendar-open" : null, forceClass));

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-date-calendar-auto-desktop");
        seq = RenderCalendarHeader(builder, seq, compact: false);
        seq = RenderCalendarBody(builder, seq, compact: false);
        builder.CloseElement();

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-date-calendar-auto-mobile");
        seq = RenderCalendarHeader(builder, seq, compact: true);
        seq = RenderCalendarBody(builder, seq, compact: true);
        builder.CloseElement();

        // Today/Clear don't depend on which variant is currently visible —
        // one shared footer, outside both, same as before auto mode existed.
        seq = RenderCalendarFooter(builder, seq);

        builder.CloseElement();
    }

    // Header: prev/next month + an editable year, replacing the scroll-driven year
    // picker of the library this was ported from with a plain stepper. Collapses to
    // just a label once compact mode's own day/month/year wheels make the stepper
    // redundant (each wheel already carries its own value).
    private int RenderCalendarHeader(RenderTreeBuilder builder, int sequence, bool compact)
    {
        var seq = sequence;

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-date-calendar-header");

        // The compact branch renders one <span>; the grid branch renders four
        // elements (nav button, span, year input, nav button) before ever
        // reaching the mode-toggle button right after this block. OpenRegion
        // isolates each branch's own sequence numbering (a fresh local counter
        // starting at 0, not `seq`) behind a single stable outer slot, so the
        // toggle button right after it always sees the same sequence number
        // regardless of which branch just rendered inside it — see this
        // method's callers for what goes wrong without that.
        builder.OpenRegion(seq++);
        if (compact)
        {
            var regionSeq = 0;
            builder.OpenElement(regionSeq++, "span");
            builder.AddAttribute(regionSeq++, "class", "fa-date-calendar-month");
            builder.AddContent(regionSeq++, "Pick a date");
            builder.CloseElement();
        }
        else
        {
            var regionSeq = 0;
            regionSeq = RenderMonthStepButton(builder, regionSeq, FaIconName.ChevronLeft, "Previous month", -1);

            builder.OpenElement(regionSeq++, "span");
            builder.AddAttribute(regionSeq++, "class", "fa-date-calendar-month");
            builder.AddContent(regionSeq++, new DateOnly(_displayYear, _displayMonth, 1).ToString("MMMM"));
            builder.CloseElement();

            builder.OpenElement(regionSeq++, "input");
            builder.AddAttribute(regionSeq++, "type", "number");
            builder.AddAttribute(regionSeq++, "class", "fa-date-calendar-year");
            builder.AddAttribute(regionSeq++, "value", _displayYear);
            builder.AddAttribute(regionSeq++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
            {
                if (int.TryParse(e.Value?.ToString(), out var year) && year >= 1)
                {
                    _displayYear = year;
                }
            }));
            builder.AddAttribute(regionSeq++, "onwheel", EventCallback.Factory.Create<WheelEventArgs>(this, HandleDisplayYearWheel));
            builder.AddEventPreventDefaultAttribute(regionSeq++, "onwheel", true);
            builder.CloseElement();

            regionSeq = RenderMonthStepButton(builder, regionSeq, FaIconName.ChevronRight, "Next month", 1);
        }

        builder.CloseRegion();

        // Hidden entirely when CompactPicker locks the mode — with nothing else to
        // switch to, a toggle button would just be dead weight in the header.
        if (!CompactPicker)
        {
            seq = RenderPickerModeToggle(builder, seq, compact);
        }

        builder.CloseElement();
        return seq;
    }

    private int RenderCalendarBody(RenderTreeBuilder builder, int sequence, bool compact)
    {
        var seq = sequence;

        // OpenRegion for the same reason RenderCalendarHeader uses one —
        // RenderWheels and the weekday-header-plus-day-grid branch consume
        // wildly different numbers of frames, and isolating each behind a
        // single stable outer slot keeps whatever follows (this method's
        // caller always closes its own wrapper element right after) from
        // seeing a sequence number that shifts depending on which branch ran.
        builder.OpenRegion(seq++);
        if (compact)
        {
            RenderWheels(builder, 0);
        }
        else
        {
            var regionSeq = 0;

            // Weekday header row.
            builder.OpenElement(regionSeq++, "div");
            builder.AddAttribute(regionSeq++, "class", "fa-date-calendar-weekdays");
            foreach (var name in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
            {
                builder.OpenElement(regionSeq++, "span");
                builder.AddContent(regionSeq++, name);
                builder.CloseElement();
            }
            builder.CloseElement();

            // Day grid: blank cells for the offset before the 1st, then one button per day.
            builder.OpenElement(regionSeq++, "div");
            builder.AddAttribute(regionSeq++, "class", "fa-date-calendar-days");

            // Keys are scoped to (year, month, cell) rather than just the day-of-month —
            // day 15 in August and day 15 in September used to share key "15", so
            // Blazor's keyed diff treated navigating months as "the same button, patch
            // its attributes" instead of "a new button". That reuse path was silently
            // dropping type/class/disabled/onclick on the reused element instead of
            // patching them (each button rendered as a bare, unstyled
            // <button>N</button>) — folding the month into the key means every cell gets
            // a key no earlier render ever used, so Blazor always creates a fresh
            // element for it instead of trying to patch one.
            var monthKeyBase = _displayYear * 10000 + _displayMonth * 100;

            var firstOfMonth = new DateOnly(_displayYear, _displayMonth, 1);
            var leadingBlanks = (int)firstOfMonth.DayOfWeek;
            for (var i = 0; i < leadingBlanks; i++)
            {
                builder.OpenElement(regionSeq++, "span");
                builder.SetKey(-(monthKeyBase + i + 1));
                builder.CloseElement();
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var daysInMonth = DateTime.DaysInMonth(_displayYear, _displayMonth);
            for (var day = 1; day <= daysInMonth; day++)
            {
                var candidate = new DateOnly(_displayYear, _displayMonth, day);
                var isSelected = Value == candidate;
                var isToday = candidate == today;
                var isDisabled = !WithinRange(candidate);
                var capturedDay = day;

                builder.OpenElement(regionSeq++, "button");
                builder.SetKey(monthKeyBase + day);
                builder.AddAttribute(regionSeq++, "type", "button");
                builder.AddAttribute(regionSeq++, "class", CssClassNames.Combine("fa-date-day", isSelected ? "fa-date-day-selected" : null, isToday && !isSelected ? "fa-date-day-today" : null));
                builder.AddAttribute(regionSeq++, "disabled", isDisabled);
                builder.AddAttribute(regionSeq++, "onclick", EventCallback.Factory.Create(this, () => SelectDay(capturedDay)));
                builder.AddContent(regionSeq++, day);
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        builder.CloseRegion();
        return seq;
    }

    // Footer: quick actions instead of the ported library's separate accordion.
    private int RenderCalendarFooter(RenderTreeBuilder builder, int sequence)
    {
        var seq = sequence;

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-date-calendar-footer");

        // Reuses the shared pill-button styles (FaButton's .fa-btn/.fa-btn-outline/
        // .fa-btn-sm from _buttons.scss) instead of a bespoke footer-button look, so
        // these two actions read as the same "button" language as the rest of FaFa
        // rather than a plain text link.
        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-btn fa-btn-outline fa-btn-sm fa-date-calendar-footer-btn");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToToday));
        builder.AddContent(seq++, "Today");
        builder.CloseElement();

        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-btn fa-btn-outline fa-btn-sm fa-date-calendar-footer-btn");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, Clear));
        builder.AddContent(seq++, "Clear");
        builder.CloseElement();

        builder.CloseElement();
        return seq;
    }

    private int RenderMonthStepButton(RenderTreeBuilder builder, int sequence, FaIconName icon, string label, int delta)
    {
        builder.OpenElement(sequence++, "button");
        builder.AddAttribute(sequence++, "type", "button");
        builder.AddAttribute(sequence++, "class", "fa-date-calendar-nav");
        builder.AddAttribute(sequence++, "aria-label", label);
        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => ChangeDisplayMonth(delta)));
        builder.OpenComponent<FaIcon>(sequence++);
        builder.AddComponentParameter(sequence++, nameof(FaIcon.Name), icon);
        builder.AddComponentParameter(sequence++, nameof(FaIcon.Color), FaIconColor.Black);
        builder.AddComponentParameter(sequence++, nameof(FaIcon.Size), 12);
        builder.CloseComponent();
        builder.CloseElement();
        return sequence;
    }

    // Same round icon-button idiom as RenderMonthStepButton, just swapping which
    // icon it shows for which mode is active rather than a fixed direction.
    // Takes the header's own `compact` explicitly rather than reading ambient
    // _compactPicker — RenderCalendar renders both the grid and wheels headers
    // every time (see its own remarks), and _compactPicker doesn't necessarily
    // match which one is actually visible right now (that's a CSS/viewport
    // question); each header's toggle button needs to offer "switch away from
    // what *this* header represents", not "switch away from whatever
    // _compactPicker currently says".
    private int RenderPickerModeToggle(RenderTreeBuilder builder, int sequence, bool compact)
    {
        builder.OpenElement(sequence++, "button");
        builder.AddAttribute(sequence++, "type", "button");
        builder.AddAttribute(sequence++, "class", "fa-date-calendar-nav fa-date-calendar-mode-toggle");
        builder.AddAttribute(sequence++, "aria-label", compact ? "Switch to calendar grid" : "Switch to compact scroll picker");
        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => SetPickerMode(!compact)));
        // Captures into whichever of _desktopModeToggleRef/_mobileModeToggleRef
        // matches this specific header — see their own remarks for why
        // SetPickerMode needs to refocus one of these after a manual click.
        builder.AddElementReferenceCapture(sequence++, r =>
        {
            if (compact)
            {
                _mobileModeToggleRef = r;
            }
            else
            {
                _desktopModeToggleRef = r;
            }
        });
        builder.OpenComponent<FaIcon>(sequence++);
        builder.AddComponentParameter(sequence++, nameof(FaIcon.Name), compact ? FaIconName.Calendar : FaIconName.Menu);
        builder.AddComponentParameter(sequence++, nameof(FaIcon.Color), FaIconColor.Black);
        builder.AddComponentParameter(sequence++, nameof(FaIcon.Size), 12);
        builder.CloseComponent();
        builder.CloseElement();
        return sequence;
    }

    // Three independently-scrollable day/month/year lists, in whatever order
    // Format put them (same _fieldOrder the split text fields above already
    // follow), each snapping to whichever item sits in the middle. Reads the
    // same _dayText/_monthText/_yearText the split fields do so switching modes
    // mid-entry doesn't lose whatever's already been typed.
    private int RenderWheels(RenderTreeBuilder builder, int sequence)
    {
        var seq = sequence;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var (selectedMonth, selectedYear, daysInMonth) = CurrentWheelBasis();
        var selectedDay = int.TryParse(_dayText, out var d) ? Math.Clamp(d, 1, daysInMonth) : Math.Min(today.Day, daysInMonth);

        // Bounded to a sane scroll length even when Min/Max are unset — nobody
        // needs to scroll through 9999 years to reach one near today.
        var minYear = Min?.Year ?? today.Year - 100;
        var maxYear = Max?.Year ?? today.Year + 50;

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-date-wheels-group");

        // A row of plain number inputs above the wheels — lets a value be typed
        // directly instead of scrolling to find it, without disturbing the
        // wheels' own row (below) whose height drives where the ::before
        // highlight band centers itself.
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-date-wheel-inputs");
        foreach (var field in _fieldOrder)
        {
            seq = RenderWheelInput(builder, seq, field);
        }
        builder.CloseElement();

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-date-wheels");

        var isFirstField = true;
        foreach (var field in _fieldOrder)
        {
            // A plain divider between each pair of columns, not the first —
            // a real sibling in this row rather than a per-wheel CSS
            // ::before/::after, since each wheel is its own overflow-y:auto
            // scroll container and an absolutely-positioned pseudo-element
            // anchored inside one scrolls along with its (far taller than
            // the visible window) content instead of staying fixed over the
            // visible band. See _date.scss's .fa-date-wheel-divider remarks.
            if (!isFirstField)
            {
                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "class", "fa-date-wheel-divider");
                builder.CloseElement();
            }

            isFirstField = false;

            // The Day wheel always renders all 31 slots — even in a 28/29/30-day
            // month — rather than Range(1, daysInMonth). Blazor's diff can't
            // cleanly reconcile a *tripled* keyed list (see RenderWheelColumn's
            // remarks) whose per-cycle length itself changes across renders —
            // switching months used to corrupt other items' rendered attributes
            // (see git history/PR discussion for the repro). A constant-length
            // list sidesteps that entirely: only which slots are enabled changes
            // now, never how many buttons exist. RenderWheelColumn disables (and
            // fa-date-wheel.js's settle() skips) whichever tail is invalid for
            // the current month.
            var (items, selected) = field switch
            {
                DateField.Month => (Enumerable.Range(1, 12).Select(v => (v, new DateOnly(2000, v, 1).ToString("MMM"))), selectedMonth),
                DateField.Day => (Enumerable.Range(1, 31).Select(v => (v, FormatFieldValue(DateField.Day, v))), selectedDay),
                _ => (Enumerable.Range(minYear, Math.Max(1, maxYear - minYear + 1)).Select(v => (v, FormatFieldValue(DateField.Year, v))), selectedYear)
            };

            // Day/Month wrap around (31 -> 1, 12 -> 1, and back) no matter which
            // direction the wheel spins — Year doesn't, it just keeps counting up
            // toward Min/Max like before. RenderWheelColumn fakes that wrap by
            // tripling the day/month list end-to-end (see its own remarks) rather
            // than anything year needs.
            var isCyclic = field is DateField.Month or DateField.Day;
            var maxValidValue = field == DateField.Day ? daysInMonth : (int?)null;
            seq = RenderWheelColumn(builder, seq, field, items, selected, isCyclic, maxValidValue);
        }

        builder.CloseElement();
        builder.CloseElement();
        return seq;
    }

    // Reuses the same digit-parsing/commit path as the split fields above the
    // popup (HandleWheelFieldInput = HandleFieldInput without the auto-advance —
    // see its own remarks) — typing here, typing up there, and scrolling a
    // wheel all stay one source of truth (_dayText/_monthText/_yearText).
    private int RenderWheelInput(RenderTreeBuilder builder, int sequence, DateField field)
    {
        builder.OpenElement(sequence++, "input");
        builder.AddAttribute(sequence++, "id", WheelInputIdFor(field));
        builder.AddAttribute(sequence++, "class", "fa-date-wheel-input");
        // Sized proportional to how many digits this field actually holds (2 for
        // month/day, 2 or 4 for year depending on Format) rather than an equal
        // one-third split — flex-grow (not a fixed width) so it still stretches
        // to fill the row, just in proportion to MaxLength instead of evenly.
        // WheelColumnFlexStyle below applies the same ratio to the wheel column
        // underneath it so the two rows stay aligned.
        builder.AddAttribute(sequence++, "style", WheelFlexStyle(field));
        builder.AddAttribute(sequence++, "type", "text");
        builder.AddAttribute(sequence++, "inputmode", "numeric");
        builder.AddAttribute(sequence++, "aria-label", field.ToString());
        builder.AddAttribute(sequence++, "maxlength", MaxLength(field));
        builder.AddAttribute(sequence++, "value", GetText(field));
        builder.AddAttribute(sequence++, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e => HandleWheelFieldInput(field, e.Value?.ToString())));
        builder.AddAttribute(sequence++, "onblur", EventCallback.Factory.Create(this, HandleFieldBlur));
        builder.AddAttribute(sequence++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, e => HandleWheelFieldKeyDown(e, field)));
        builder.AddAttribute(sequence++, "onwheel", EventCallback.Factory.Create<WheelEventArgs>(this, e => HandleFieldWheel(field, e)));
        builder.AddEventPreventDefaultAttribute(sequence++, "onwheel", true);
        builder.CloseElement();
        return sequence;
    }

    // Same digit-count-proportional flex-grow for both the typed-number row and
    // the wheel column beneath it, so a wider (year) field lines up with its own
    // wider column instead of both rows splitting evenly into equal thirds.
    private string WheelFlexStyle(DateField field) => $"flex-grow:{MaxLength(field)};";

    // Up/Down arrow spin on a single wheel-column input, scoped to that field
    // alone (mirrors HandleFieldKeyDown for the split fields above the popup) —
    // deliberately doesn't wire Left/Right the way HandleFieldKeyDown does, since
    // that hops focus to _dayRef/_monthRef/_yearRef (the split fields), which
    // would yank focus out of the popup the user is actively scrolling in.
    // AdjustField's resulting highlight change is enough on its own — no re-center
    // call needed here, see fa-date-wheel.js's MutationObserver.
    private void HandleWheelFieldKeyDown(KeyboardEventArgs e, DateField field)
    {
        switch (e.Key)
        {
            case "ArrowUp": AdjustField(field, 1); break;
            case "ArrowDown": AdjustField(field, -1); break;
        }
    }

    // Repeat count for a cyclic (Month/Day) wheel's item list, and which copy of
    // it is the "real" one carrying the fa-date-wheel-item-selected class.
    // fa-date-wheel.js's checkWrap() silently jumps scrollTop by one copy's
    // height whenever the wheel drifts into the first or last copy, snapping it
    // back into this middle one — because every copy renders identical text,
    // that jump is visually seamless, so the wheel *looks* infinite in both
    // directions even though the DOM behind it is just three finite laps.
    // Three copies (not two) so there's always a full copy's worth of scroll
    // buffer on both sides of the current position before a jump is needed,
    // even mid-flick. Only one copy gets the selected class — every copy would
    // otherwise satisfy `item.Value == selected` at once, and fa-date-wheel.js's
    // MutationObserver re-centers on *every* element that gains that class, so
    // more than one match per wheel races to scroll it to two different places.
    private const int WheelCycleCount = 3;
    private const int WheelCenterCycleIndex = 1;

    private int RenderWheelColumn(RenderTreeBuilder builder, int sequence, DateField field, IEnumerable<(int Value, string Text)> items, int selected, bool isCyclic, int? maxValidValue)
    {
        var seq = sequence;
        var materializedItems = items as IReadOnlyList<(int Value, string Text)> ?? items.ToList();
        var cycleCount = isCyclic ? WheelCycleCount : 1;

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-date-wheel", isCyclic ? "fa-date-wheel-cyclic" : null));
        builder.AddAttribute(seq++, "style", WheelFlexStyle(field));
        builder.AddAttribute(seq++, "aria-label", $"{field} scroll picker");

        for (var cycle = 0; cycle < cycleCount; cycle++)
        {
            foreach (var item in materializedItems)
            {
                // A non-cyclic wheel (Year) only ever renders a single copy at
                // cycle 0 — WheelCenterCycleIndex (1) is a *cyclic*-wheel
                // concept (which of the 3 tripled copies is "the real one"),
                // so gating on it unconditionally meant Year's one and only
                // copy could never match and never got marked selected at
                // all, leaving the wheel stuck wherever it happened to mount
                // (usually scrollTop 0, the earliest year) instead of ever
                // landing on the current/bound year.
                var isSelected = item.Value == selected && (!isCyclic || cycle == WheelCenterCycleIndex);
                // Day always renders all 31 slots regardless of the selected
                // month (see RenderWheels' remarks on why item *count* has to
                // stay constant across renders) — the tail beyond the current
                // month's real day count (29-31 in February, say) is disabled
                // instead of omitted: same idiom the day-grid already uses for
                // an out-of-range day (see .fa-date-day:disabled), and
                // fa-date-wheel.js's settle() skips disabled items when
                // picking whichever one to commit, so a spin can't rest on one.
                var isDisabled = maxValidValue.HasValue && item.Value > maxValidValue.Value;
                var capturedValue = item.Value;

                builder.OpenElement(seq++, "button");
                builder.SetKey((((long)field * 100_000) + item.Value) + ((long)cycle * 1_000_000));
                builder.AddAttribute(seq++, "type", "button");
                builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-date-wheel-item", isSelected ? "fa-date-wheel-item-selected" : null));
                builder.AddAttribute(seq++, "disabled", isDisabled);
                // Always wired up, even for a currently-disabled slot — every
                // item keeps the exact same attribute shape every render (only
                // the boolean/string *values* change) rather than some renders
                // omitting this attribute outright, which is what actually
                // corrupted sibling items' own attributes when the Day wheel's
                // item count still varied by month (see the shape-stability
                // remarks above `maxValidValue`). The disabled attribute alone
                // already stops the browser from ever firing a click here.
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => SelectWheelValue(field, capturedValue)));
                builder.AddContent(seq++, item.Text);
                builder.CloseElement();
            }
        }

        builder.CloseElement();
        return seq;
    }

    private string WheelInputIdFor(DateField field) => field switch
    {
        DateField.Month => _wheelMonthInputId,
        DateField.Day => _wheelDayInputId,
        _ => _wheelYearInputId
    };
}
