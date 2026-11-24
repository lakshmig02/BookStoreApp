using BookStore.Application.Features.Genres.GetGenreSummary;
using BookStore.Domain.Books;
using BookStore.Infrastructure.Persistence;
using BookStore.Tests.Support;

namespace BookStore.Tests.Infrastructure;

public sealed class GenreSummaryReaderTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Raw_sql_aggregate_counts_books_and_stock_per_genre_and_handler_zero_fills()
    {
        await _database.SeedAsync("A", Genre.SciFi, stock: 3);
        await _database.SeedAsync("B", Genre.SciFi, stock: 4);
        await _database.SeedAsync("C", Genre.History, stock: 0);

        await using var db = _database.CreateContext();
        var summary = await new GetGenreSummaryHandler(new GenreSummaryReader(db))
            .Handle(new GetGenreSummaryQuery(), CancellationToken.None);

        Assert.Equal(3, summary.TotalBooks);
        Assert.Equal(Enum.GetValues<Genre>(), summary.Genres.Select(g => g.Genre));
        Assert.Contains(new GenreCountDto(Genre.SciFi, 2, 7), summary.Genres);
        Assert.Contains(new GenreCountDto(Genre.History, 1, 0), summary.Genres);
        Assert.Contains(new GenreCountDto(Genre.Fantasy, 0, 0), summary.Genres);
    }
}
