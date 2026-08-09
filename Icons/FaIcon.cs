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
            case FaIconName.ChevronRight:
                Path("M8.6 7.4 10 6l6 6-6 6-1.4-1.4L13.2 12z");
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
            case FaIconName.Calendar:
                FillRulePath("M5 7h14v13H5V7zm2 3h10v8H7v-8z");
                Rect("8", "4", "1.6", "4", "0.8");
                Rect("14.4", "4", "1.6", "4", "0.8");
                break;
            case FaIconName.Edit:
                Path("M3 21l1.3-5.3L15.4 4.6a1.5 1.5 0 0 1 2.1 0l1.9 1.9a1.5 1.5 0 0 1 0 2.1L8.3 19.7 3 21zM16.5 5.7l1.8 1.8 1.3-1.3a.5.5 0 0 0 0-.7l-1.1-1.1a.5.5 0 0 0-.7 0z");
                break;
            case FaIconName.Save:
                FillRulePath("M4 3h12.2L20 6.8V21H4V3zm2 2v5h9V5h-2.2v3H8V5H6zm.5 9v5h11v-5h-11z");
                break;
            case FaIconName.Delete:
                Rect("3", "6", "18", "2", "1");
                Rect("9.5", "3", "5", "2.4", "1");
                FillRulePath("M5.5 9h13l-1.2 11.2a1 1 0 0 1-1 .8H7.7a1 1 0 0 1-1-.8zM10 11v7h1.4v-7zm2.6 0v7h1.4v-7z");
                break;
            case FaIconName.Cancel:
                Path("M17.4 4.6 12 10 6.6 4.6 4.6 6.6 10 12 4.6 17.4 6.6 19.4 12 14 17.4 19.4 19.4 17.4 14 12 19.4 6.6z");
                break;
            case FaIconName.Confirm:
                Path("M9.5 16.2 5.3 12l-1.4 1.4L9.5 19 20.1 8.4l-1.4-1.4z");
                break;
            case FaIconName.Search:
                FillRulePath("M10 3.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13zm0 2a4.5 4.5 0 1 1 0 9 4.5 4.5 0 0 1 0-9z");
                Rect("14.3", "15", "2", "7.5", "1", "rotate(45 15.3 18.75)");
                break;
            case FaIconName.Filter:
                Path("M3 4h18l-7 8.5V19l-4 2v-8.5z");
                break;
            case FaIconName.Sort:
                Path("M7 3 3 8h3v5h2V8h3z");
                Path("M17 21 21 16h-3v-5h-2v5h-3z");
                break;
            case FaIconName.Download:
                Path("M11 3h2v9.2l3.6-3.6 1.4 1.4-6 6-6-6 1.4-1.4L11 12.2z");
                Rect("4", "19", "16", "2", "1");
                break;
            case FaIconName.Upload:
                Path("M13 21h-2v-9.2l-3.6 3.6-1.4-1.4 6-6 6 6-1.4 1.4L13 11.8z");
                Rect("4", "19", "16", "2", "1");
                break;
            case FaIconName.Copy:
                FillRulePath("M9 3h10a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1h-1V6H9zM4 8h10a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V9a1 1 0 0 1 1-1zm1.6 1.6v9h7.8v-9z");
                break;
            case FaIconName.Print:
                Rect("6", "3", "12", "6", "1");
                FillRulePath("M4 9h16a1 1 0 0 1 1 1v6a1 1 0 0 1-1 1h-3v4H7v-4H4a1 1 0 0 1-1-1v-6a1 1 0 0 1 1-1zm4 8v3h8v-3z");
                break;
            case FaIconName.Refresh:
                Path("M12 4V1L8 5l4 4V6a6 6 0 1 1-6 6H4a8 8 0 1 0 8-8z");
                break;
            case FaIconName.Settings:
                Rect("11", "1", "2", "3", "1");
                Rect("11", "1", "2", "3", "1", "rotate(45 12 12)");
                Rect("11", "1", "2", "3", "1", "rotate(90 12 12)");
                Rect("11", "1", "2", "3", "1", "rotate(135 12 12)");
                Rect("11", "1", "2", "3", "1", "rotate(180 12 12)");
                Rect("11", "1", "2", "3", "1", "rotate(225 12 12)");
                Rect("11", "1", "2", "3", "1", "rotate(270 12 12)");
                Rect("11", "1", "2", "3", "1", "rotate(315 12 12)");
                FillRulePath("M12 7.5a4.5 4.5 0 1 0 0 9 4.5 4.5 0 0 0 0-9zm0 2.2a2.3 2.3 0 1 1 0 4.6 2.3 2.3 0 0 1 0-4.6z");
                break;
            case FaIconName.Menu:
                Rect("4", "6", "16", "2", "1");
                Rect("4", "11", "16", "2", "1");
                Rect("4", "16", "16", "2", "1");
                break;
            case FaIconName.MoreHorizontal:
                Circle("5", "12", "1.8");
                Circle("12", "12", "1.8");
                Circle("19", "12", "1.8");
                break;
            case FaIconName.MoreVertical:
                Circle("12", "5", "1.8");
                Circle("12", "12", "1.8");
                Circle("12", "19", "1.8");
                break;
            case FaIconName.Warning:
                FillRulePath("M12 3 22 20H2zm-.9 6.2v5.4h1.8V9.2zm0 7.2v1.8h1.8v-1.8z");
                break;
            case FaIconName.Info:
                FillRulePath("M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm-.9 4.6v1.8h1.8V6.6zm0 4v6.8h1.8v-6.8z");
                break;
            case FaIconName.Success:
                FillRulePath("M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm5.6 6.4-6.4 6.4-2.8-2.8 1.3-1.3 1.5 1.5 5.1-5.1z");
                break;
            case FaIconName.Error:
                FillRulePath("M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm3.5 5.7 1.8 1.8L13.8 12l3.5 3.5-1.8 1.8L12 13.8l-3.5 3.5-1.8-1.8L10.2 12 6.7 8.5l1.8-1.8L12 10.2z");
                break;
            case FaIconName.EyeOff:
                FillRulePath("M12 6c-5 0-8.5 3.4-9.6 5.6a1 1 0 0 0 0 .8C3.5 14.6 7 18 12 18s8.5-3.4 9.6-5.6a1 1 0 0 0 0-.8C20.5 9.4 17 6 12 6zm0 9.6a3.6 3.6 0 1 1 0-7.2 3.6 3.6 0 0 1 0 7.2z");
                Path("M4.6 3.2 3.2 4.6 19.4 20.8 20.8 19.4z");
                break;
            case FaIconName.Star:
                Path("M12 2.5l2.9 6 6.6.7-4.9 4.5 1.3 6.5L12 16.9l-5.9 3.3 1.3-6.5-4.9-4.5 6.6-.7z");
                break;
            case FaIconName.Bell:
                Path("M12 2.5a1.3 1.3 0 0 1 1.3 1.3v.7c2.9.6 5 3.2 5 6.3v4l2 3H3.7l2-3v-4c0-3.1 2.1-5.7 5-6.3v-.7A1.3 1.3 0 0 1 12 2.5z");
                Path("M9.7 20a2.3 2.3 0 0 0 4.6 0z");
                break;
            case FaIconName.Lock:
                FillRulePath("M5 10h14v11H5V10zm7 2.5a1.3 1.3 0 1 0 0 2.6 1.3 1.3 0 0 0 0-2.6z");
                FillRulePath("M7.5 10V7a4.5 4.5 0 1 1 9 0v3h-2V7a2.5 2.5 0 1 0-5 0v3z");
                break;
            case FaIconName.Unlock:
                FillRulePath("M5 10h14v11H5V10zm7 2.5a1.3 1.3 0 1 0 0 2.6 1.3 1.3 0 0 0 0-2.6z");
                FillRulePath("M6 9.5V7a4.5 4.5 0 0 1 9 0h-2a2.5 2.5 0 0 0-5 0v2.5z");
                break;
            case FaIconName.ArrowUp:
                Path("M11 20V7.8l-4.6 4.6L5 11l7-7 7 7-1.4 1.4L13 7.8V20z");
                break;
            case FaIconName.ArrowDown:
                Path("M13 4v12.2l4.6-4.6L19 13l-7 7-7-7 1.4-1.4 4.6 4.6V4z");
                break;
            case FaIconName.ArrowLeft:
                Path("M20 13H7.8l4.6 4.6L11 19l-7-7 7-7 1.4 1.4L7.8 11H20z");
                break;
            case FaIconName.ArrowRight:
                Path("M4 11h12.2l-4.6-4.6L13 5l7 7-7 7-1.4-1.4 4.6-4.6H4z");
                break;
            case FaIconName.ChevronUp:
                Path("M7.4 15.4 6 14l6-6 6 6-1.4 1.4L12 10.8z");
                break;
            case FaIconName.ChevronDown:
                Path("M7.4 8.6 6 10l6 6 6-6-1.4-1.4L12 13.2z");
                break;
            case FaIconName.Undo:
                Path("M12.5 8c-2.65 0-5.05.99-6.9 2.6L2 7v9h9l-3.62-3.62c1.39-1.16 3.16-1.88 5.12-1.88 3.54 0 6.55 2.31 7.6 5.5l2.37-.78C21.08 11.03 17.15 8 12.5 8z");
                break;
            case FaIconName.Redo:
                Path("M18.4 10.6C16.55 8.99 14.15 8 11.5 8c-4.65 0-8.58 3.03-9.96 7.22L3.9 16c1.05-3.19 4.05-5.5 7.6-5.5 1.95 0 3.73.72 5.12 1.88L13 16h9V7z");
                break;
            case FaIconName.Login:
                FillRulePath("M20 3h-9v2h7v14h-7v2h9V3z");
                Path("M2.6 12h8.2l-2.6-2.6 1.4-1.4L14.6 12 10 16.4l-1.4-1.4 2.6-2.6H2.6z");
                break;
            case FaIconName.Logout:
                FillRulePath("M4 3h9v2H6v14h7v2H4V3z");
                Path("M11 12h8.2l-2.6-2.6 1.4-1.4L22.4 12 18 16.4l-1.4-1.4 2.6-2.6H11z");
                break;
            case FaIconName.Share:
                Circle("18", "5", "2.2");
                Circle("18", "19", "2.2");
                Circle("6", "12", "2.2");
                Rect("6.5", "11.2", "13", "1.6", "0.8", "rotate(-25 13 12)");
                Rect("6.5", "11.2", "13", "1.6", "0.8", "rotate(25 13 12)");
                break;
            case FaIconName.ExternalLink:
                FillRulePath("M5 5h7v2H7v10h10v-5h2v7H5V5z");
                Path("M13 3h8v8h-2V6.4l-7.3 7.3-1.4-1.4L17.6 5H13z");
                break;
        }

        builder.CloseElement();
    }
}
