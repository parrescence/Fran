namespace FactoryAspects.Components;

/// <summary>
/// What <see cref="FaLoginForm.OnSubmit"/> hands back — the entered credentials, not
/// a result. FaLoginForm never talks to an auth endpoint itself; the caller's
/// <c>OnSubmit</c> handler is what actually authenticates and decides what happens
/// next (a wrong-password message is the caller's <see cref="FaLoginForm.ErrorText"/>
/// to set, not something FaLoginForm derives on its own).
/// </summary>
public sealed class FaLoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}
