namespace Fran.Validation;

/// <summary>
/// Builds the effective, resolved <see cref="FaValidationBuilder{TModel}"/> for one
/// <c>EditContext</c>'s model: root tier first (whatever <see cref="IFaValidator{TModel}"/>
/// is registered in DI for <c>TModel</c>, if any), then the form
/// tier's own override delegate on top. Stateless/static — this isn't itself a
/// long-lived service, just the two-step resolution <c>FaModelValidator&lt;TModel&gt;</c>
/// runs once when it mounts.
/// </summary>
internal static class FaValidationResolver
{
    public static FaValidationBuilder<TModel> Resolve<TModel>(
        IServiceProvider services,
        Action<FaValidationBuilder<TModel>>? formOverride) where TModel : class
    {
        var builder = new FaValidationBuilder<TModel>();

        // Root tier — optional: a model with no registered IFaValidator<TModel> just
        // means nothing declares rules for it here, not an error. DataAnnotations
        // (via the stock DataAnnotationsValidator FaForm already renders alongside
        // this) still applies independently either way.
        var rootValidator = services.GetService(typeof(IFaValidator<TModel>)) as IFaValidator<TModel>;
        rootValidator?.Configure(builder);

        // Form tier — runs after, so it can add/replace/remove anything the root
        // validator just declared. See FaValidationBuilder's own remarks on why
        // that's the entire "override" mechanism, not a merge algorithm.
        formOverride?.Invoke(builder);

        return builder;
    }
}
