namespace FaFa.Validation;

/// <summary>
/// One implementation per model type — the root/DTO tier of FaFa's validation
/// system, direct analogue of EF Core's <c>IEntityTypeConfiguration&lt;TEntity&gt;</c>:
/// a separate class that declares a model's validation rules instead of attributes
/// scattered across its properties, registered once via
/// <see cref="FaValidationServiceCollectionExtensions.AddFaValidator{TModel,TValidator}"/>
/// and picked up automatically by every <see cref="Components.FaModelValidator{TModel}"/>
/// for that model type.
/// </summary>
/// <remarks>
/// Deliberately not a fluent <c>RuleFor(x =&gt; x.Y)</c> expression-tree DSL — see
/// <see cref="FaValidationBuilder{TModel}.Field{TValue}"/>'s own remarks for why. A
/// consumer wanting per-form or per-element rule overrides on top of whatever this
/// declares doesn't touch this class at all — see <c>FaModelValidator&lt;TModel&gt;.
/// ConfigureValidation</c> (form tier) and each input component's own <c>Validate</c>
/// parameter (element tier).
/// </remarks>
public interface IFaValidator<TModel> where TModel : class
{
    void Configure(FaValidationBuilder<TModel> builder);
}
