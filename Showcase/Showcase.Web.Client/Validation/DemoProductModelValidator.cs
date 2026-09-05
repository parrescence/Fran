using Fran.Validation;

namespace Showcase.Web.Client.Validation;

/// <summary>
/// The root/DTO tier for <see cref="DemoProductModel"/> — registered once in
/// Program.cs (<c>AddFaValidator&lt;DemoProductModel, DemoProductModelValidator&gt;()</c>),
/// applies to every <c>FaModelValidator&lt;DemoProductModel&gt;</c> anywhere in the
/// app unless a specific form overrides a field (see <c>FaValidationPage</c>'s own
/// <c>ConfigureValidation</c>). The direct analogue of an EF Core
/// <c>IEntityTypeConfiguration&lt;DemoProductModel&gt;</c>.
/// </summary>
public sealed class DemoProductModelValidator : IFaValidator<DemoProductModel>
{
    public void Configure(FaValidationBuilder<DemoProductModel> builder)
    {
        builder.Field(nameof(DemoProductModel.Name), m => m.Name)
            .Required("Name is required.")
            .MaxLength(40); // shorthand over Must — see FaFieldValidationBuilderExtensions

        builder.Field(nameof(DemoProductModel.Price), m => m.Price)
            .Between(0.01m, 10_000m, "Price must be between $0.01 and $10,000.");

        // Between works on any IComparable<TValue>, not just decimal — int here.
        builder.Field(nameof(DemoProductModel.Quantity), m => m.Quantity)
            .Between(1, 100, "Quantity must be between 1 and 100.");

        // EqualTo — exact match against a fixed value, not a range/predicate.
        builder.Field(nameof(DemoProductModel.Country), m => m.Country)
            .EqualTo("US", "Only US is supported right now.");

        // Sku deliberately has no rule here — FaValidationPage's own Sku field
        // uses an element-tier Validate override instead, so this class stays a
        // clean example of "what the model always requires everywhere."
    }
}
