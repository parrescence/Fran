namespace Fran.Components;

/// <summary>
/// Fire a toast notification from anywhere in a consuming app: <c>[Inject]</c> this,
/// call <see cref="Show"/>, and whichever <c>&lt;FaToastHost /&gt;</c> is mounted
/// (typically once, near the app's root layout) picks it up through a plain C# event.
/// Register it once — <c>builder.Services.AddScoped&lt;FaToastService&gt;();</c> —
/// same as any other injectable service, no custom extension method to remember.
///
/// An earlier version of this was a `static` event instead, to skip the DI
/// registration step — but that made it process-wide rather than scoped to one
/// user's session, so on Blazor Server, one user's toast reached every other user's
/// browser too. Registered `Scoped` (the standard lifetime for anything tied to one
/// user's session/circuit — see Microsoft's Blazor DI docs), it's automatically
/// session-isolated on Server and equivalent to a singleton on WebAssembly (one user
/// per process there already), with no special-casing needed either way.
/// </summary>
public sealed class FaToastService
{
    public event Action<FaToastMessage>? OnShow;

    public void Show(string text, FaAlertVariant variant = FaAlertVariant.Info, int durationMs = 4000)
        => OnShow?.Invoke(new FaToastMessage(text, variant, durationMs));
}
