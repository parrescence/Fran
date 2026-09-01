using Microsoft.Extensions.DependencyInjection;

namespace FaFa.Validation;

/// <summary>
/// Registers a root-tier <see cref="IFaValidator{TModel}"/> — the FaFa-validation
/// equivalent of EF Core's <c>modelBuilder.ApplyConfiguration(new XConfiguration())</c>,
/// just done once at startup instead of per-<c>DbContext</c>-build. Scoped, matching
/// <c>FaToastService</c>'s own documented lifetime (this repo's one other DI-registered
/// type) — a validator instance is stateless per-request configuration, not
/// per-user data, so the choice costs nothing but stays consistent with that
/// precedent rather than introducing a second convention.
/// </summary>
public static class FaValidationServiceCollectionExtensions
{
    public static IServiceCollection AddFaValidator<TModel, TValidator>(this IServiceCollection services)
        where TModel : class
        where TValidator : class, IFaValidator<TModel>
        => services.AddScoped<IFaValidator<TModel>, TValidator>();
}
