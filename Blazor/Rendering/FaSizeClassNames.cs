namespace FaFa.Rendering;

/// <summary>
/// Maps <see cref="FaFa.Components.FaSize"/> to a component's own <c>{prefix}-{suffix}</c>
/// CSS class — <c>FaSize.Medium</c> returns <c>null</c> (no class; it's the default,
/// unsized look every component already had before <c>Size</c> existed), so
/// <c>CssClassNames.Combine("fa-btn", FaSizeClassNames.Class("fa-btn", Size), ...)</c>
/// is the one line each component needs.
/// </summary>
internal static class FaSizeClassNames
{
    public static string? Class(string prefix, Components.FaSize size)
    {
        var suffix = size switch
        {
            Components.FaSize.XSmall => "xs",
            Components.FaSize.Small => "sm",
            Components.FaSize.Large => "lg",
            Components.FaSize.XLarge => "xl",
            _ => null
        };

        return suffix is null ? null : $"{prefix}-{suffix}";
    }
}
