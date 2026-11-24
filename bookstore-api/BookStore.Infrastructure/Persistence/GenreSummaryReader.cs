using BookStore.Application.Features.Genres.GetGenreSummary;
using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Persistence;

/// <summary>
/// The hand-written SQL query in this project: a GROUP BY aggregate over the Books table.
/// </summary>
internal sealed class GenreSummaryReader(BookStoreDbContext db) : IGenreSummaryReader
{
    private const string Sql = """
        SELECT  b."Genre"                    AS "Genre",
                COUNT(*)                     AS "BookCount",
                COALESCE(SUM(b."Stock"), 0)  AS "TotalStock"
        FROM    "Books" AS b
        GROUP BY b."Genre"
        """;

    public async Task<IReadOnlyList<GenreCountDto>> GetGenreCountsAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Database.SqlQueryRaw<GenreCountRow>(Sql).ToListAsync(cancellationToken);

        return rows
            .Where(r => Enum.TryParse<Genre>(r.Genre, out _))
            .Select(r => new GenreCountDto(Enum.Parse<Genre>(r.Genre), r.BookCount, r.TotalStock))
            .ToList();
    }

    private sealed record GenreCountRow(string Genre, int BookCount, int TotalStock);
}
