using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Fran.Components;

/// <summary>
/// Fran-styled stand-in for the stock <c>ValidationMessage&lt;TValue&gt;</c> — same
/// <see cref="For"/> shape (an <c>Expression&lt;Func&lt;TValue&gt;&gt;</c> purely for
/// <c>FieldIdentifier.Create</c>'s own field-identity extraction, not the
/// rule-authoring DSL Fran's validation system otherwise avoids expression trees
/// for — see <c>FaValidationBuilder.Field</c>'s remarks), rendered as a plain
/// <c>.fa-validation-message</c> div instead of the built-in component's bare
/// <c>&lt;ul&gt;</c>. Every Fran input already renders its own inline message
/// automatically (its <c>ShowValidationMessage</c> parameter, on by default) — this
/// exists for a consumer who wants a field's error shown somewhere other than
/// directly under that field (its own <c>ShowValidationMessage="false"</c> plus one
/// of these placed elsewhere), not as something every form needs to add by hand.
/// </summary>
public sealed class FaValidationMessage<TValue> : ComponentBase
{
    [CascadingParameter] private EditContext CurrentEditContext { get; set; } = default!;
    [Parameter, EditorRequired] public Expression<Func<TValue>> For { get; set; } = default!;
    [Parameter] public string? CssClass { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (CurrentEditContext is null)
        {
            return;
        }

        var identifier = FieldIdentifier.Create(For);
        var messages = CurrentEditContext.GetValidationMessages(identifier).ToList();
        if (messages.Count == 0)
        {
            return;
        }

        var seq = 0;
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", string.IsNullOrWhiteSpace(CssClass) ? "fa-validation-message" : $"fa-validation-message {CssClass}");
        builder.AddContent(seq++, messages[0]);
        builder.CloseElement();
    }
}
