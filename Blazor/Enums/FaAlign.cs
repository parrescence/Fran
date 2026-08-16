namespace FaFa.Components;

/// <summary>
/// Flex <c>justify-content</c> alignment for a row of elements — <see
/// cref="FaModal"/>'s footer button row and <see cref="FaForm{TModel}"/>'s action
/// bar both take this instead of each inventing their own alignment options.
/// </summary>
public enum FaAlign
{
    /// <summary>justify-content: flex-start</summary>
    Start,
    /// <summary>justify-content: center</summary>
    Center,
    /// <summary>justify-content: flex-end — the default for both FaModal's footer and FaForm's action bar.</summary>
    End,
    /// <summary>justify-content: space-between</summary>
    Between,
    /// <summary>justify-content: space-around</summary>
    Around,
    /// <summary>justify-content: space-evenly</summary>
    Evenly
}
