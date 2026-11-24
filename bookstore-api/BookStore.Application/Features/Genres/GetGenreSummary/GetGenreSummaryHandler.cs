using BookStore.Application.Abstractions.Messaging;
using BookStore.Domain.Books;

namespace BookStore.Application.Features.Genres.GetGenreSummary;

internal sealed class GetGenreSummaryHandler(IGenreSummaryReader reader)
    : IRequestHandler<GetGenreSummaryQuery, GenreSummaryDto>
{
    public async Task<GenreSummaryDto> Handle(GetGenreSummaryQuery query, CancellationToken cancellationToken)
    {
        var counts = (await reader.GetGenreCountsAsync(cancellationToken)).ToDictionary(c => c.Genre);

        // Every genre in the controlled vocabulary is returned (zero-filled) in declaration order,
        // so the filter list is stable regardless of what is in stock.
        var genres = Enum.GetValues<Genre>()
            .Select(g => counts.GetValueOrDefault(g) ?? new GenreCountDto(g, 0, 0))
            .ToList();

        return new GenreSummaryDto(genres.Sum(g => g.BookCount), genres);
    }
}
