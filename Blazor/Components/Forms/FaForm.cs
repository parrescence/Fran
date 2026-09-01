using FaFa.Rendering;
using FaFa.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace FaFa.Components;

/// <summary>
/// A self-contained wrapper around <see cref="EditForm"/>: hand it a <typeparamref
/// name="TModel"/> and your own field markup (ordinary <c>FaInput</c>/<c>FaSelect</c>/
/// etc. with <c>@bind-Value</c> straight to the model, written normally in your own
/// .razor file — FaForm doesn't generate fields itself, it just supplies the
/// EditContext they bind against), and it hands the populated model back via <see
/// cref="OnSubmit"/> once validation passes. Adds the boilerplate every hand-rolled
/// EditForm otherwise repeats: <see cref="DataAnnotationsValidator"/> +
/// <see cref="ValidationSummary"/>, and a Cancel/Delete/Submit button row whose
/// label and visible actions follow <see cref="Mode"/>.
/// </summary>
public sealed class FaForm<TModel> : ComponentBase where TModel : class
{
    [Parameter, EditorRequired] public TModel Model { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<TModel> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }

    /// <summary>Create (default) vs Edit — only changes the default SubmitText ("Create"/"Save") and whether the Delete button shows.</summary>
    [Parameter] public FaFormMode Mode { get; set; } = FaFormMode.Create;

    [Parameter] public string? SubmitText { get; set; }
    [Parameter] public string CancelText { get; set; } = "Cancel";
    [Parameter] public string DeleteText { get; set; } = "Delete";

    /// <summary>Disables the Submit button and swaps its text to "Saving…" while a caller's async OnSubmit handler is still in flight. FaForm doesn't set this itself — the caller flips it around its own await.</summary>
    [Parameter] public bool Busy { get; set; }

    [Parameter] public bool ShowValidationSummary { get; set; } = true;

    /// <summary>
    /// Opts this form into FaFa's own validation system (see <c>FaFa.Validation</c>)
    /// alongside the <c>DataAnnotationsValidator</c> this component already always
    /// renders — off by default since not every <typeparamref name="TModel"/> has a
    /// registered <see cref="IFaValidator{TModel}"/>, and turning it on for one that
    /// doesn't would just silently do nothing rather than being a useful default.
    /// </summary>
    [Parameter] public bool UseFaValidation { get; set; }

    /// <summary>The form tier of FaFa's validation system — see <c>FaModelValidator&lt;TModel&gt;.ConfigureValidation</c>. Ignored unless <see cref="UseFaValidation"/> is set.</summary>
    [Parameter] public Action<FaValidationBuilder<TModel>>? ConfigureValidation { get; set; }
    [Parameter] public FaAlign ButtonAlign { get; set; } = FaAlign.End;
    [Parameter] public string? CssClass { get; set; }

    private string EffectiveSubmitText => SubmitText ?? (Mode == FaFormMode.Edit ? "Save" : "Create");

    private Task HandleValidSubmit() => OnSubmit.InvokeAsync(Model);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<EditForm>(0);
        builder.AddComponentParameter(1, nameof(EditForm.Model), Model);
        builder.AddComponentParameter(2, nameof(EditForm.OnValidSubmit),
            EventCallback.Factory.Create<EditContext>(this, _ => HandleValidSubmit()));
        builder.AddAttribute(3, "class", CssClassNames.Combine("fa-form", CssClass));
        builder.AddComponentParameter(4, nameof(EditForm.ChildContent), (RenderFragment<EditContext>)(_ => BuildBody));
        builder.CloseComponent();
    }

    private void BuildBody(RenderTreeBuilder builder)
    {
        var seq = 0;

        if (ShowValidationSummary)
        {
            builder.OpenComponent<DataAnnotationsValidator>(seq++);
            builder.CloseComponent();

            // Independent of DataAnnotationsValidator above — both populate the same
            // EditContext ValidationMessageStore, so every FaFa input's own inline
            // error display shows whichever kind of rule actually fired regardless
            // of which of these rendered it. Nested inside ShowValidationSummary's
            // own check (rather than a separate top-level `if`) since that
            // parameter already doubles as "run any validator here at all," not
            // just "show the summary list" — UseFaValidation follows the same rule.
            if (UseFaValidation)
            {
                builder.OpenComponent<FaModelValidator<TModel>>(seq++);
                builder.AddComponentParameter(seq++, nameof(FaModelValidator<TModel>.ConfigureValidation), ConfigureValidation);
                builder.CloseComponent();
            }

            builder.OpenComponent<ValidationSummary>(seq++);
            builder.CloseComponent();
        }

        builder.AddContent(seq++, ChildContent);

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", CssClassNames.Combine("fa-form-actions", FaAlignClassNames.ToClass(ButtonAlign)));

        if (OnDelete.HasDelegate && Mode == FaFormMode.Edit)
        {
            builder.OpenComponent<FaButton>(seq++);
            builder.AddComponentParameter(seq++, nameof(FaButton.Variant), FaButtonVariant.OutlineDanger);
            builder.AddComponentParameter(seq++, nameof(FaButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => OnDelete.InvokeAsync()));
            builder.AddComponentParameter(seq++, nameof(FaButton.ChildContent), (RenderFragment)(b => b.AddContent(0, DeleteText)));
            builder.CloseComponent();
        }

        if (OnCancel.HasDelegate)
        {
            builder.OpenComponent<FaButton>(seq++);
            builder.AddComponentParameter(seq++, nameof(FaButton.Variant), FaButtonVariant.Outline);
            builder.AddComponentParameter(seq++, nameof(FaButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => OnCancel.InvokeAsync()));
            builder.AddComponentParameter(seq++, nameof(FaButton.ChildContent), (RenderFragment)(b => b.AddContent(0, CancelText)));
            builder.CloseComponent();
        }

        builder.OpenComponent<FaButton>(seq++);
        builder.AddComponentParameter(seq++, nameof(FaButton.Type), "submit");
        builder.AddComponentParameter(seq++, nameof(FaButton.Disabled), Busy);
        builder.AddComponentParameter(seq++, nameof(FaButton.ChildContent), (RenderFragment)(b => b.AddContent(0, Busy ? "Saving…" : EffectiveSubmitText)));
        builder.CloseComponent();

        builder.CloseElement();
    }
}
