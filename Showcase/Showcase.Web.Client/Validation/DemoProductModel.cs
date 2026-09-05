namespace Showcase.Web.Client.Validation;

/// <summary>
/// The model behind <c>FaValidationPage</c> — deliberately carries no
/// <c>System.ComponentModel.DataAnnotations</c> attributes at all, so every error
/// the demo page shows comes from Fran's own validation system
/// (<see cref="DemoProductModelValidator"/> for the root tier, the page's own
/// <c>ConfigureValidation</c> delegate for the form tier, and one field's own
/// <c>Validate</c> parameter for the element tier), not from
/// <c>DataAnnotationsValidator</c> riding along for free.
/// </summary>
public sealed class DemoProductModel
{
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Country { get; set; } = "";

    /// <summary>Deliberately has no root-tier rule — only the demo page's own form-tier ConfigureValidation declares one for this, so trying it shows a rule that exists *only* because that one form asked for it.</summary>
    public string Category { get; set; } = "";
}
