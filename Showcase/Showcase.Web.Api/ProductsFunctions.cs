using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Showcase.Web.Api;

/// <summary>
/// In-memory only, no auth — exists purely so the showcase client has something
/// real to call for its "ItemsProvider"/"pulled from the database" demos (FaGrid,
/// FaCarousel). See <see cref="SampleData"/>.
/// </summary>
public sealed class ProductsFunctions
{
    // FaCarousel.ItemsProvider mode — one fetch of the whole slide set, no paging.
    [Function("GetAllProducts")]
    public async Task<HttpResponseData> GetAllProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(SampleData.Products);
        return response;
    }

    // FaGrid.ItemsProvider mode — page index/size, sort, and a status filter,
    // mirroring FaGridRequest's shape as query params since this is a plain GET,
    // not a POST body.
    [Function("GetProductsPage")]
    public async Task<HttpResponseData> GetProductsPage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/page")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var pageIndex = int.TryParse(query["pageIndex"], out var pi) ? pi : 0;
        var pageSize = int.TryParse(query["pageSize"], out var ps) ? ps : 10;
        var sortKey = query["sortKey"];
        var sortAscending = bool.TryParse(query["sortAscending"], out var sa) && sa;
        var statusFilter = query["statusFilter"];

        IEnumerable<Product> products = SampleData.Products;

        if (!string.IsNullOrEmpty(statusFilter))
        {
            products = products.Where(p => statusFilter == "in-stock" ? p.InStock : !p.InStock);
        }

        products = sortKey switch
        {
            "Name" => sortAscending ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name),
            "Price" => sortAscending ? products.OrderBy(p => p.Price) : products.OrderByDescending(p => p.Price),
            _ => products
        };

        var all = products.ToList();
        var page = all.Skip(pageIndex * pageSize).Take(pageSize).ToList();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new ProductPageResult(page, all.Count));
        return response;
    }
}
