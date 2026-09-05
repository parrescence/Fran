namespace Fran.Rendering;

/// <summary>
/// Shared class-list join helper for components that build their <c>class</c> attribute
/// from several optional pieces (a base class, a variant class, a caller-supplied class,
/// ...). Centralizes what several ported components used to do with ad hoc string
/// splitting/concatenation.
/// </summary>
/// <remarks>
/// Named <c>CssClassNames</c> rather than plain <c>ClassNames</c> deliberately — this is
/// still an ordinary C# class (a "class" in the .NET sense), and <c>ClassNames</c> reads
/// ambiguous next to that. Not <c>CssClass</c> either: most components already expose a
/// <c>CssClass</c> parameter, and a same-named type would be shadowed by that parameter
/// inside the component's own methods.
/// </remarks>
internal static class CssClassNames
{
    /// <summary>
    /// Joins the non-empty/non-whitespace parts with a single space, in order. Null or
    /// blank parts are dropped, so callers can pass conditional expressions directly
    /// (<c>CssClassNames.Combine("fa-btn", Small ? "fa-btn-sm" : null, CssClass)</c>)
    /// without each one needing its own null check.
    /// </summary>
    public static string Combine(params string?[] parts) =>
        string.Join(' ', parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}
