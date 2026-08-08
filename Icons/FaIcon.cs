using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FactoryAspects.Icons;

/// <summary>
/// Small hand-drawn icon set — no third-party icon font/library, matching the
/// "no third-party styling dependency" rule Bootstrap was removed under. Every path
/// fills with `currentColor`, so the two color variants are pure CSS
/// (.fa-icon-white / .fa-icon-black in theme.css), not per-icon markup.
/// </summary>
public sealed class FaIcon : ComponentBase
{
    [Parameter, EditorRequired] public FaIconName Name { get; set; }
    [Parameter] public FaIconColor Color { get; set; } = FaIconColor.White;
    [Parameter] public int Size { get; set; } = 20;
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? CssClass { get; set; }

    private string ColorClass => Color == FaIconColor.Black ? "fa-icon-black" : "fa-icon-white";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var hasTitle = !string.IsNullOrEmpty(Title);

        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "class", $"fa-icon {ColorClass} {CssClass}");
        builder.AddAttribute(2, "viewBox", "0 0 24 24");
        builder.AddAttribute(3, "width", Size);
        builder.AddAttribute(4, "height", Size);
        builder.AddAttribute(5, "aria-hidden", hasTitle ? null : "true");
        builder.AddAttribute(6, "role", hasTitle ? "img" : null);

        var seq = 7;

        if (hasTitle)
        {
            builder.OpenElement(seq++, "title");
            builder.AddContent(seq++, Title);
            builder.CloseElement();
        }

        void Path(string d)
        {
            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "d", d);
            builder.CloseElement();
        }

        void FillRulePath(string d)
        {
            builder.OpenElement(seq++, "path");
            builder.AddAttribute(seq++, "fill-rule", "evenodd");
            builder.AddAttribute(seq++, "clip-rule", "evenodd");
            builder.AddAttribute(seq++, "d", d);
            builder.CloseElement();
        }

        void Rect(string x, string y, string width, string height, string? rx = null, string? transform = null)
        {
            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "x", x);
            builder.AddAttribute(seq++, "y", y);
            builder.AddAttribute(seq++, "width", width);
            builder.AddAttribute(seq++, "height", height);
            builder.AddAttribute(seq++, "rx", rx);
            builder.AddAttribute(seq++, "transform", transform);
            builder.CloseElement();
        }

        void Circle(string cx, string cy, string r)
        {
            builder.OpenElement(seq++, "circle");
            builder.AddAttribute(seq++, "cx", cx);
            builder.AddAttribute(seq++, "cy", cy);
            builder.AddAttribute(seq++, "r", r);
            builder.CloseElement();
        }

        switch (Name)
        {
            case FaIconName.Home:
                Path("M12 3.2 3 10.6V21h6.5v-7.5h5V21H21V10.6z");
                break;
            case FaIconName.Plus:
                Path("M11 3h2v8h8v2h-8v8h-2v-8H3v-2h8z");
                break;
            case FaIconName.Ledger:
                Rect("4", "5", "16", "2.6", "1.3");
                Rect("4", "10.7", "16", "2.6", "1.3");
                Rect("4", "16.4", "10", "2.6", "1.3");
                break;
            case FaIconName.PiggyBank:
                Path("M12 4.2c-4.4 0-8 2.9-8 6.4 0 1.6.7 3 1.8 4.1L5.2 18h3l.5-1.3c1 .3 2.1.4 3.3.4s2.3-.1 3.3-.4l.5 1.3h3l-.6-3.3c1.1-1.1 1.8-2.5 1.8-4.1 0-3.5-3.6-6.4-8-6.4z");
                Circle("16.6", "9.4", "1.1");
                Rect("9.8", "6.2", "4", "1.4", "0.7");
                break;
            case FaIconName.Dashboard:
                Rect("4", "13", "4", "7", "1");
                Rect("10", "8", "4", "12", "1");
                Rect("16", "4", "4", "16", "1");
                break;
            case FaIconName.People:
                Circle("9", "8", "3");
                Path("M3 19.5c0-3.3 2.7-6 6-6s6 2.7 6 6z");
                Circle("17.5", "9", "2.3");
                Path("M14.6 13.8c1-.5 2.1-.8 2.9-.8 2.8 0 5 2.3 5 5v1.5h-4.2v-1.5c0-1.7-.7-3.2-1.8-4.2z");
                break;
            case FaIconName.Sun:
                Circle("12", "12", "4.5");
                Rect("11", "1", "2", "4", "1", "rotate(0 12 12)");
                Rect("11", "1", "2", "4", "1", "rotate(45 12 12)");
                Rect("11", "1", "2", "4", "1", "rotate(90 12 12)");
                Rect("11", "1", "2", "4", "1", "rotate(135 12 12)");
                Rect("11", "1", "2", "4", "1", "rotate(180 12 12)");
                Rect("11", "1", "2", "4", "1", "rotate(225 12 12)");
                Rect("11", "1", "2", "4", "1", "rotate(270 12 12)");
                Rect("11", "1", "2", "4", "1", "rotate(315 12 12)");
                break;
            case FaIconName.Moon:
                Path("M20 15A8 8 0 1 1 9 4 6.4 6.4 0 0 0 20 15z");
                break;
            case FaIconName.Eye:
                FillRulePath("M12 6c-5 0-8.5 3.4-9.6 5.6a1 1 0 0 0 0 .8C3.5 14.6 7 18 12 18s8.5-3.4 9.6-5.6a1 1 0 0 0 0-.8C20.5 9.4 17 6 12 6zm0 9.6a3.6 3.6 0 1 1 0-7.2 3.6 3.6 0 0 1 0 7.2z");
                break;
            case FaIconName.ChevronLeft:
                Path("M15.4 7.4 14 6l-6 6 6 6 1.4-1.4L10.8 12z");
                break;
            case FaIconName.Tag:
                FillRulePath("M12.6 3.4 20.6 3l-.4 8-9.4 9.4a1.4 1.4 0 0 1-2 0l-6-6a1.4 1.4 0 0 1 0-2zM17 8.4a1.6 1.6 0 1 0 0-3.2 1.6 1.6 0 0 0 0 3.2z");
                break;
            case FaIconName.Person:
                Circle("12", "7.5", "3.8");
                Path("M4.5 19.8c0-4.1 3.4-7.4 7.5-7.4s7.5 3.3 7.5 7.4V21h-15z");
                break;
            case FaIconName.Receipt:
                FillRulePath("M6 2.5h12a1 1 0 0 1 1 1V21l-2.2-1.3-2.1 1.3-2.2-1.3-2.2 1.3-2.1-1.3L5 21V3.5a1 1 0 0 1 1-1zm1.8 5.2h8.4v1.6H7.8zm0 4h8.4v1.6H7.8zm0 4h5.6v1.6H7.8z");
                break;
        }

        builder.CloseElement();
    }
}
