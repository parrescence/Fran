using Showcase.Web.Api;

var builder = WebApplication.CreateBuilder(args);

// Wide open for local dev only — this API has no auth and isn't meant to be
// deployed as-is; it exists purely so the showcase client has something real to
// call for its "ItemsProvider"/"pull from the database" demos (FaGrid, FaCarousel,
// FaLoginForm). Tighten this (and add real auth) before this ever runs anywhere
// but localhost.
const string DevClientCorsPolicy = "DevClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevClientCorsPolicy, policy => policy
        .WithOrigins("https://localhost:7095", "http://localhost:5193")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors(DevClientCorsPolicy);
app.UseHttpsRedirection();

var products = SampleData.Products;

// FaCarousel.ItemsProvider mode — one fetch of the whole slide set, no paging.
app.MapGet("/api/products", () => Results.Ok(products))
    .WithName("GetAllProducts");

// FaGrid.ItemsProvider mode — page index/size, sort, and a status filter, mirroring
// FaGridRequest's shape as query params since this is a plain GET, not a POST body.
app.MapGet("/api/products/page", (int pageIndex, int pageSize, string? sortKey, bool sortAscending, string? statusFilter) =>
{
    IEnumerable<Product> query = products;

    if (!string.IsNullOrEmpty(statusFilter))
    {
        query = query.Where(p => statusFilter == "in-stock" ? p.InStock : !p.InStock);
    }

    query = sortKey switch
    {
        "Name" => sortAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
        "Price" => sortAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
        _ => query
    };

    var all = query.ToList();
    var page = all.Skip(pageIndex * pageSize).Take(pageSize).ToList();

    return Results.Ok(new ProductPageResult(page, all.Count));
})
    .WithName("GetProductsPage");

// FaLoginForm demo — a fake auth check, no real user store. "demo"/"password" is
// the one combination that succeeds; everything else fails.
app.MapPost("/api/auth/login", (LoginRequest request) =>
{
    var success = request.Username == "demo" && request.Password == "password";
    return Results.Ok(new LoginResult(success, success ? null : "Incorrect username or password."));
})
    .WithName("Login");

app.Run();
