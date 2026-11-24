using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Features.Genres.GetGenreSummary;

namespace BookStore.Api.Endpoints;

internal static class GenresEndpoints
{
    public static IEndpointRouteBuilder MapGenresEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/genres", async (ISender sender, CancellationToken ct) =>
                TypedResults.Ok(await sender.Send(new GetGenreSummaryQuery(), ct)))
            .WithTags("Genres")
            .WithName("GetGenreSummary")
            .WithSummary("Book count and stock per genre (hand-written SQL aggregate)");
        return app;
    }
}
