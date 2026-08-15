namespace FaFa.Components;

/// <summary>
/// Which edge of an <see cref="FaProgress"/> track the fill grows from. Right/Left
/// are horizontal (the track's width is its extent); Up/Down are vertical (the
/// track's height is its extent).
/// </summary>
public enum FaProgressDirection
{
    /// <summary>Horizontal, fills from the left edge toward the right.</summary>
    Right,
    /// <summary>Horizontal, fills from the right edge toward the left.</summary>
    Left,
    /// <summary>Vertical, fills from the bottom edge upward.</summary>
    Up,
    /// <summary>Vertical, fills from the top edge downward.</summary>
    Down
}
