namespace Fran.Components;

/// <summary>
/// One toast's content and how long it stays up (in milliseconds) before
/// FaToastHost auto-dismisses it. Raised through FaToastService.Show(...);
/// consumed by whichever FaToastHost is currently mounted.
/// </summary>
public sealed record FaToastMessage(string Text, FaAlertVariant Variant, int DurationMs);
