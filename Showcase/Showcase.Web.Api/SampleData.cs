namespace Showcase.Web.Api;

public sealed record Product(int Id, string Name, string Category, decimal Price, bool InStock);

public sealed record ProductPageResult(IReadOnlyList<Product> Items, int TotalCount);

public sealed record LoginRequest(string Username, string Password, bool RememberMe);

public sealed record LoginResult(bool Success, string? ErrorMessage);

/// <summary>In-memory only — this exists purely to give the showcase client's grid/carousel/dropdown "pulled from the database" demos something real to fetch.</summary>
internal static class SampleData
{
    public static readonly IReadOnlyList<Product> Products = new[]
    {
        new Product(1, "Trail Runner Jacket", "Apparel", 129.99m, true),
        new Product(2, "Cedar Camp Mug", "Outdoor", 18.50m, true),
        new Product(3, "Alpine Wool Socks", "Apparel", 14.00m, true),
        new Product(4, "Folding Camp Stool", "Outdoor", 42.00m, false),
        new Product(5, "Rain Shell, Packable", "Apparel", 89.00m, true),
        new Product(6, "Titanium Spork", "Outdoor", 9.99m, true),
        new Product(7, "Wool Beanie", "Apparel", 22.00m, true),
        new Product(8, "Camp Lantern, Solar", "Outdoor", 34.99m, false),
        new Product(9, "Merino Base Layer", "Apparel", 64.00m, true),
        new Product(10, "Insulated Water Bottle", "Outdoor", 27.50m, true),
        new Product(11, "Trekking Poles, Pair", "Outdoor", 58.00m, true),
        new Product(12, "Fleece Vest", "Apparel", 47.00m, false),
    };
}
