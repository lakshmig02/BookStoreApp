using BookStore.Api.Http;
using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Features.Catalog.SearchCatalog;

namespace BookStore.Api.Endpoints;

/// <summary>Server-side proxy to Open Library. The browser never calls Open Library directly.</summary>
internal static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/catalog/search", SearchCatalog)
            .WithTags("Catalog")
            .WithName("SearchCatalog")
            .WithSummary("Search Open Library (proxied); results are not persisted")
            .Produces<CatalogSearchResultDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
        return app;
    }

    private static async Task<IResult> SearchCatalog(string? q, int? page, int? pageSize, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new SearchCatalogQuery(q ?? string.Empty, page ?? 1, pageSize ?? 10), ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }
}
