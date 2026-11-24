namespace BookStore.Application.Abstractions.Catalog;

/// <summary>
/// Port to an external book catalog (Open Library in production). Used only for discovery:
/// results are transient and never read again once a book is in the store.
/// </summary>
public interface IBookCatalog
{
    /// <exception cref="CatalogUnavailableException">The catalog timed out, failed, or returned garbage.</exception>
    Task<CatalogSearchPage> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken);
}

/// <summary>A normalized catalog search hit — the shape our store understands, not Open Library's.</summary>
public sealed record CatalogBook(
    string Key,
    string Title,
    string Author,
    string? Isbn,
    int? PublishedYear,
    string? CoverUrl);

public sealed record CatalogSearchPage(IReadOnlyList<CatalogBook> Items, int TotalCount);

public sealed class CatalogUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
