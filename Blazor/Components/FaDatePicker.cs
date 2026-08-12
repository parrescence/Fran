using System.Diagnostics.CodeAnalysis;
using FactoryAspects.Icons;
using FactoryAspects.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FactoryAspects.Components;

/// <summary>
/// Split day/month/year date entry with a calendar popup, min/max range support, and
/// an optional floating label. A clean rewrite of an older library's date picker: the
/// original idea (three separately-editable, arrow-key-navigable number fields plus a
/// calendar grid) is worth keeping, but this version has no reflection, no ids
/// regenerated on every render, and no JS interop — the popup's open/closed state is
/// plain Blazor state, and "close when focus leaves the control" is a native
/// <c>@onfocusout</c>-with-a-grace-period pattern (<see cref="HandleFocusOutAsync"/>)
/// instead of a document click listener reaching back into Blazor over JS interop.
/// </summary>
public sealed class FaDatePicker : InputBase<DateOnly?>
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

    private enum DateField { Day, Month, Year }

    private readonly string _wrapperId = $"fa-datepicker-{Guid.NewGuid():N}";
    private readonly string _dayId = $"fa-datepicker-day-{Guid.NewGuid():N}";
    private readonly string _monthId = $"fa-datepicker-month-{Guid.NewGuid():N}";
    private readonly string _yearId = $"fa-datepicker-year-{Guid.NewGuid():N}";

    private ElementReference _dayRef;
    private ElementReference _monthRef;
    private ElementReference _yearRef;

    private string _dayText = "";
    private string _monthText = "";
    private string _yearText = "";

    private bool _isOpen;
    private int _displayYear;
    private int _displayMonth;
    private bool _displayInitialized;
    private CancellationTokenSource? _pendingClose;

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
        SyncTextFromValue();

        if (!_displayInitialized)
        {
            var basis = Value ?? DateOnly.FromDateTime(DateTime.Today);
            _displayYear = basis.Year;
            _displayMonth = basis.Month;
            _displayInitialized = true;
        }
    }

    private void SyncTextFromValue()
    {
        if (Value is { } date)
        {
            _dayText = date.Day.ToString("D2");
            _monthText = date.Month.ToString("D2");
            _yearText = date.Year.ToString("D4");
        }
        else
        {
            _dayText = "";
            _monthText = "";
            _yearText = "";
        }
    }

    private bool WithinRange(DateOnly date) => (Min is null || date >= Min) && (Max is null || date <= Max);

    private void HandleFieldInput(DateField field, string? rawValue)
    {
        var digits = new string((rawValue ?? "").Where(char.IsDigit).ToArray());
        var maxLength = field == DateField.Year ? 4 : 2;
        if (digits.Length > maxLength)
        {
            digits = digits[..maxLength];
        }

        switch (field)
        {
            case DateField.Day: _dayText = digits; break;
            case DateField.Month: _monthText = digits; break;
            case DateField.Year: _yearText = digits; break;
        }

        TryCommitValue();

        // Auto-advance to the next field once this one is full, same idea as a
        // native multi-part date input.
        if (digits.Length == maxLength)
        {
            _ = field switch
            {
                DateField.Month => _dayRef.FocusAsync(),
                DateField.Day => _yearRef.FocusAsync(),
                _ => ValueTask.CompletedTask
            };
        }
    }

    private void TryCommitValue()
    {
        if (string.IsNullOrEmpty(_dayText) && string.IsNullOrEmpty(_monthText) && string.IsNullOrEmpty(_yearText))
        {
            CurrentValue = null;
            return;
        }

        if (int.TryParse(_dayText, out var day) && int.TryParse(_monthText, out var month) &&
            int.TryParse(_yearText, out var year) && year >= 1 &&
            month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month))
        {
            var candidate = new DateOnly(year, month, day);
            if (WithinRange(candidate))
            {
                CurrentValue = candidate;
                _displayYear = year;
                _displayMonth = month;
            }
        }

        // Incomplete or currently out-of-range digits: leave CurrentValue where it is
        // and let the user keep typing rather than fighting them mid-entry.
    }

    private void AdjustField(DateField field, int delta)
    {
        switch (field)
        {
            case DateField.Year:
            {
                var year = int.TryParse(_yearText, out var y) ? y + delta : DateTime.Today.Year;
                _yearText = Math.Max(1, year).ToString("D4");
                break;
            }
            case DateField.Month:
            {
                var month = (int.TryParse(_monthText, out var m) ? m : DateTime.Today.Month) + delta;
                month = ((month - 1 + 12) % 12) + 1;
                _monthText = month.ToString("D2");
                break;
            }
            case DateField.Day:
            {
                var maxDay = int.TryParse(_yearText, out var yy) && int.TryParse(_monthText, out var mm)
                    ? DateTime.DaysInMonth(yy, mm)
                    : 31;
                var day = (int.TryParse(_dayText, out var d) ? d : DateTime.Today.Day) + delta;
                day = ((day - 1 + maxDay) % maxDay) + 1;
                _dayText = day.ToString("D2");
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
        }
    }

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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var hasLabel = !string.IsNullOrEmpty(Label);

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", CssClassNames.Combine("fa-field", ContainerCssClass));

        if (hasLabel && !FloatingLabel)
        {
            builder.OpenElement(2, "label");
            builder.AddAttribute(3, "class", "fa-label");
            builder.AddContent(4, Label);
            builder.CloseElement();
        }

        if (ReadOnly)
        {
            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "fa-readonly-flat");
            builder.AddContent(7, Value?.ToString("MMMM d, yyyy") ?? "");
            builder.CloseElement();
            builder.CloseElement();
            return;
        }

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "id", _wrapperId);
        builder.AddAttribute(7, "class", CssClassNames.Combine("fa-datepicker", FloatingLabel ? "fa-datepicker-floating" : null, Value.HasValue ? "fa-datepicker-has-value" : null));
        builder.AddAttribute(8, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleWrapperKeyDown));
        builder.AddAttribute(9, "onfocusin", EventCallback.Factory.Create(this, HandleFocusIn));
        builder.AddAttribute(10, "onfocusout", EventCallback.Factory.Create(this, HandleFocusOutAsync));

        builder.OpenElement(11, "div");
        builder.AddAttribute(12, "class", "fa-datepicker-fields");

        RenderField(builder, 13, DateField.Month, _monthId, "mm", 2, _monthText, r => _monthRef = r);
        RenderSeparator(builder, 20);
        RenderField(builder, 21, DateField.Day, _dayId, "dd", 2, _dayText, r => _dayRef = r);
        RenderSeparator(builder, 30);
        RenderField(builder, 31, DateField.Year, _yearId, "yyyy", 4, _yearText, r => _yearRef = r);

        builder.CloseElement();

        builder.OpenElement(40, "button");
        builder.AddAttribute(41, "type", "button");
        builder.AddAttribute(42, "class", "fa-datepicker-toggle");
        builder.AddAttribute(43, "aria-label", _isOpen ? "Close calendar" : "Open calendar");
        builder.AddAttribute(44, "onclick", EventCallback.Factory.Create(this, ToggleOpen));
        builder.OpenComponent<FaIcon>(45);
        builder.AddComponentParameter(46, nameof(FaIcon.Name), FaIconName.Calendar);
        builder.AddComponentParameter(47, nameof(FaIcon.Color), FaIconColor.Black);
        builder.AddComponentParameter(48, nameof(FaIcon.Size), 16);
        builder.CloseComponent();
        builder.CloseElement();

        if (hasLabel && FloatingLabel)
        {
            builder.OpenElement(49, "label");
            builder.AddAttribute(50, "class", "fa-label fa-datepicker-floating-label");
            builder.AddContent(51, Label);
            builder.CloseElement();
        }

        RenderCalendar(builder, 60);

        builder.CloseElement();
        builder.CloseElement();
    }

    private void RenderSeparator(RenderTreeBuilder builder, int sequence)
    {
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", "fa-datepicker-separator");
        builder.AddAttribute(sequence + 2, "aria-hidden", "true");
        builder.AddContent(sequence + 3, "/");
        builder.CloseElement();
    }

    private void RenderField(RenderTreeBuilder builder, int sequence, DateField field, string id, string placeholder, int maxLength, string value, Action<ElementReference> captureRef)
    {
        builder.OpenElement(sequence, "input");
        builder.AddAttribute(sequence + 1, "id", id);
        builder.AddAttribute(sequence + 2, "class", "fa-datepicker-input");
        builder.AddAttribute(sequence + 3, "type", "text");
        builder.AddAttribute(sequence + 4, "inputmode", "numeric");
        builder.AddAttribute(sequence + 5, "placeholder", placeholder);
        builder.AddAttribute(sequence + 6, "maxlength", maxLength);
        builder.AddAttribute(sequence + 7, "value", value);
        builder.AddAttribute(sequence + 8, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e => HandleFieldInput(field, e.Value?.ToString())));
        builder.AddAttribute(sequence + 9, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, e => HandleFieldKeyDown(e, field)));
        builder.AddElementReferenceCapture(sequence + 10, captureRef);
        builder.CloseElement();
    }

    private void RenderCalendar(RenderTreeBuilder builder, int sequence)
    {
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", CssClassNames.Combine("fa-datepicker-calendar", _isOpen ? "fa-datepicker-calendar-open" : null));

        // Header: prev/next month + an editable year, replacing the scroll-driven year
        // picker of the library this was ported from with a plain stepper.
        builder.OpenElement(sequence + 2, "div");
        builder.AddAttribute(sequence + 3, "class", "fa-datepicker-calendar-header");

        RenderMonthStepButton(builder, sequence + 4, FaIconName.ChevronLeft, "Previous month", -1);

        builder.OpenElement(sequence + 10, "span");
        builder.AddAttribute(sequence + 11, "class", "fa-datepicker-calendar-month");
        builder.AddContent(sequence + 12, new DateOnly(_displayYear, _displayMonth, 1).ToString("MMMM"));
        builder.CloseElement();

        builder.OpenElement(sequence + 13, "input");
        builder.AddAttribute(sequence + 14, "type", "number");
        builder.AddAttribute(sequence + 15, "class", "fa-datepicker-calendar-year");
        builder.AddAttribute(sequence + 16, "value", _displayYear);
        builder.AddAttribute(sequence + 17, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
        {
            if (int.TryParse(e.Value?.ToString(), out var year) && year >= 1)
            {
                _displayYear = year;
            }
        }));
        builder.CloseElement();

        RenderMonthStepButton(builder, sequence + 18, FaIconName.ChevronRight, "Next month", 1);

        builder.CloseElement();

        // Weekday header row.
        builder.OpenElement(sequence + 25, "div");
        builder.AddAttribute(sequence + 26, "class", "fa-datepicker-calendar-weekdays");
        var seq = sequence + 27;
        foreach (var name in new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" })
        {
            builder.OpenElement(seq++, "span");
            builder.AddContent(seq++, name);
            builder.CloseElement();
        }
        builder.CloseElement();

        // Day grid: blank cells for the offset before the 1st, then one button per day.
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-datepicker-calendar-days");

        var firstOfMonth = new DateOnly(_displayYear, _displayMonth, 1);
        var leadingBlanks = (int)firstOfMonth.DayOfWeek;
        for (var i = 0; i < leadingBlanks; i++)
        {
            builder.OpenElement(seq++, "span");
            builder.CloseElement();
        }

        var daysInMonth = DateTime.DaysInMonth(_displayYear, _displayMonth);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var candidate = new DateOnly(_displayYear, _displayMonth, day);
            var isSelected = Value == candidate;
            var isDisabled = !WithinRange(candidate);
            var capturedDay = day;

            builder.OpenElement(seq++, "button");
            builder.SetKey(day);
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-datepicker-day", isSelected ? "fa-datepicker-day-selected" : null));
            builder.AddAttribute(seq++, "disabled", isDisabled);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => SelectDay(capturedDay)));
            builder.AddContent(seq++, day);
            builder.CloseElement();
        }

        builder.CloseElement();

        // Footer: quick actions instead of the ported library's separate accordion.
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-datepicker-calendar-footer");

        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-datepicker-calendar-footer-btn");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, GoToToday));
        builder.AddContent(seq++, "Today");
        builder.CloseElement();

        builder.OpenElement(seq++, "button");
        builder.AddAttribute(seq++, "type", "button");
        builder.AddAttribute(seq++, "class", "fa-datepicker-calendar-footer-btn");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, Clear));
        builder.AddContent(seq++, "Clear");
        builder.CloseElement();

        builder.CloseElement();

        builder.CloseElement();
    }

    private void RenderMonthStepButton(RenderTreeBuilder builder, int sequence, FaIconName icon, string label, int delta)
    {
        builder.OpenElement(sequence, "button");
        builder.AddAttribute(sequence + 1, "type", "button");
        builder.AddAttribute(sequence + 2, "class", "fa-datepicker-calendar-nav");
        builder.AddAttribute(sequence + 3, "aria-label", label);
        builder.AddAttribute(sequence + 4, "onclick", EventCallback.Factory.Create(this, () => ChangeDisplayMonth(delta)));
        builder.OpenComponent<FaIcon>(sequence + 5);
        builder.AddComponentParameter(sequence + 6, nameof(FaIcon.Name), icon);
        builder.AddComponentParameter(sequence + 7, nameof(FaIcon.Color), FaIconColor.Black);
        builder.AddComponentParameter(sequence + 8, nameof(FaIcon.Size), 12);
        builder.CloseComponent();
        builder.CloseElement();
    }
}
