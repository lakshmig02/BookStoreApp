using BookStore.Domain.Books;

namespace BookStore.Application.Features.Genres.GetGenreSummary;

/// <summary>
/// Slice-owned read port. Implemented in Infrastructure with a hand-written SQL aggregate,
/// which is why it is not expressed through <c>IBookStoreDbContext</c>.
/// </summary>
public interface IGenreSummaryReader
{
    /// <summary>Returns one row per genre that has at least one book.</summary>
    Task<IReadOnlyList<GenreCountDto>> GetGenreCountsAsync(CancellationToken cancellationToken);
}
