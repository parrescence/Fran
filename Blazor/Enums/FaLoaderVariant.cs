namespace FaFa.Components;

/// <summary>
/// Shared color variant for <see cref="FaProgress"/>, <see cref="FaSpinner"/>,
/// <see cref="FaHelixLoader"/>, and <see cref="FaPongLoader"/> — same "one enum, one
/// set of CSS classes" shape as <see cref="FaBadgeVariant"/>, just named for this
/// group of components since a spinner or progress bar isn't a badge. No separate
/// "Success" member — <see cref="Accent"/> already is the success-toned green every
/// other component (FaAlert/FaBadge) uses for that meaning, via --fa-accent.
/// </summary>
public enum FaLoaderVariant
{
    /// <summary>--fa-accent — the default. Same green used for "success" everywhere else.</summary>
    Accent,
    /// <summary>--fa-primary.</summary>
    Primary,
    /// <summary>--fa-gold.</summary>
    Gold,
    /// <summary>--fa-ember — the danger-toned red, e.g. a budget progress bar that's gone over.</summary>
    Danger
}
