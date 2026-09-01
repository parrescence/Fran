using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace FaFa.Rendering;

/// <summary>
/// Shared by every FaFa input component's own inline validation display
/// (<c>ShowValidationMessage</c>/<c>Validate</c> — see <c>FaInput</c>/<c>FaSelect</c>/
/// etc.) so the "combine EditContext's model/form-tier messages with this one
/// element's own override result, then render the first one" logic exists in one
/// place instead of six near-identical copies. Takes <c>EditContext</c>/
/// <c>FieldIdentifier</c> as plain parameters rather than trying to reach into an
/// <c>InputBase&lt;TValue&gt;</c> instance from outside — those are <c>protected</c>
/// members, only readable by the component itself (which already has them via its
/// own inheritance), not by a helper class no amount of "this."-style extension
/// syntax can grant access to.
/// </summary>
internal static class FaValidationMessageRenderer
{
    public static IReadOnlyList<string> Resolve(EditContext? editContext, FieldIdentifier fieldIdentifier, bool show, string? elementMessage)
    {
        List<string>? messages = null;

        if (show && editContext is not null)
        {
            foreach (var message in editContext.GetValidationMessages(fieldIdentifier))
            {
                (messages ??= []).Add(message);
            }
        }

        if (elementMessage is not null)
        {
            (messages ??= []).Add(elementMessage);
        }

        return (IReadOnlyList<string>?)messages ?? [];
    }

    public static void Render(RenderTreeBuilder builder, IReadOnlyList<string> messages)
    {
        if (messages.Count == 0)
        {
            return;
        }

        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", "fa-validation-message");
        builder.AddContent(seq++, messages[0]);
        builder.CloseElement();
    }
}
