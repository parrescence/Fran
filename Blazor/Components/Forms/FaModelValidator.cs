using Fran.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Fran.Components;

/// <summary>
/// Runs Fran's own validation rules — the root/DTO tier from a registered
/// <see cref="IFaValidator{TModel}"/> plus this instance's own <see cref="ConfigureValidation"/>
/// form-tier override — against an <c>EditContext</c>'s model, the same way the stock
/// <c>DataAnnotationsValidator</c> does for attribute-decorated properties. Placed
/// alongside (not instead of) <c>DataAnnotationsValidator</c> inside an
/// <c>EditForm</c> — both populate the same <c>ValidationMessageStore</c>, so every
/// Fran input's own inline error display (<c>ShowValidationMessage</c>, see
/// <c>FaInput</c>/<c>FaSelect</c>/etc.) shows whichever kind of rule actually fired,
/// with no extra wiring needed to tell the two apart. <c>FaForm&lt;TModel&gt;</c>
/// renders this automatically when its own <c>UseFaValidation</c> is set.
/// </summary>
public sealed class FaModelValidator<TModel> : ComponentBase, IDisposable where TModel : class
{
    [CascadingParameter] private EditContext CurrentEditContext { get; set; } = default!;
    [Inject] private IServiceProvider Services { get; set; } = default!;

    /// <summary>The form tier — runs after whatever the registered <see cref="IFaValidator{TModel}"/> declares, on the same builder, so it can add to, replace, or remove any of that model's rules for just this one form instance.</summary>
    [Parameter] public Action<FaValidationBuilder<TModel>>? ConfigureValidation { get; set; }

    private ValidationMessageStore _messageStore = default!;
    private FaValidationBuilder<TModel> _builder = default!;

    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException($"{nameof(FaModelValidator<TModel>)} requires a cascading EditContext — place it inside an EditForm.");
        }

        if (CurrentEditContext.Model is not TModel)
        {
            throw new InvalidOperationException(
                $"{nameof(FaModelValidator<TModel>)}<{typeof(TModel).Name}> was placed inside an EditForm " +
                $"whose Model is a {CurrentEditContext.Model.GetType().Name}, not a {typeof(TModel).Name}.");
        }

        _messageStore = new ValidationMessageStore(CurrentEditContext);
        CurrentEditContext.OnValidationRequested += HandleValidationRequested;
        CurrentEditContext.OnFieldChanged += HandleFieldChanged;
    }

    // Resolution (root tier + form-tier ConfigureValidation) runs here instead of
    // OnInitialized — OnParametersSet re-runs on every render, so a ConfigureValidation
    // delegate that changes (or a swapped Services scope) never leaves a stale rule
    // set behind, the same "don't just resolve once at mount" reasoning FaDate's own
    // OnParametersSet-driven state uses elsewhere in this library. Still runs once
    // before the first render too, since OnParametersSet always fires after
    // OnInitialized even on mount — nothing here depends on _builder before this
    // has had its first chance to run.
    protected override void OnParametersSet()
    {
        _builder = FaValidationResolver.Resolve<TModel>(Services, ConfigureValidation);
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        _messageStore.Clear();
        var model = (TModel)CurrentEditContext.Model;

        foreach (var (propertyName, rules) in _builder.Rules)
        {
            AddMessages(propertyName, model, rules);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (!_builder.Rules.TryGetValue(e.FieldIdentifier.FieldName, out var rules))
        {
            return;
        }

        _messageStore.Clear(e.FieldIdentifier);
        AddMessages(e.FieldIdentifier.FieldName, (TModel)CurrentEditContext.Model, rules);
        CurrentEditContext.NotifyValidationStateChanged();
    }

    private void AddMessages(string propertyName, TModel model, List<Func<TModel, string?>> rules)
    {
        var identifier = new FieldIdentifier(model, propertyName);
        foreach (var rule in rules)
        {
            if (rule(model) is { } message)
            {
                _messageStore.Add(identifier, message);
            }
        }
    }

    void IDisposable.Dispose()
    {
        if (CurrentEditContext is not null)
        {
            CurrentEditContext.OnValidationRequested -= HandleValidationRequested;
            CurrentEditContext.OnFieldChanged -= HandleFieldChanged;
        }
    }
}
