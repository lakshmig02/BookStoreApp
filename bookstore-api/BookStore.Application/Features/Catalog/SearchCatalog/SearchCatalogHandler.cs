using BookStore.Application.Abstractions.Catalog;
using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Abstractions.Persistence;
using BookStore.Application.Common;
using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Catalog.SearchCatalog;

internal sealed class SearchCatalogHandler(IBookCatalog catalog, IBookStoreDbContext db)
    : IRequestHandler<SearchCatalogQuery, Result<CatalogSearchResultDto>>
{
    public static readonly Error Unavailable = Error.Unavailable(
        "Catalog.Unavailable", "The book catalog (Open Library) is unavailable right now. Please try again shortly.");

    public async Task<Result<CatalogSearchResultDto>> Handle(SearchCatalogQuery query, CancellationToken cancellationToken)
    {
        CatalogSearchPage page;
        try
        {
            page = await catalog.SearchAsync(query.Q.Trim(), query.Page, query.PageSize, cancellationToken);
        }
        catch (CatalogUnavailableException)
        {
            // Already logged with detail by the catalog adapter; here it is an expected outcome.
            return Unavailable;
        }

        var keys = page.Items.Select(i => i.Key).ToList();
        var isbns = page.Items.Select(i => Book.NormalizeIsbn(i.Isbn)).OfType<string>().ToList();

        var owned = await db.Books.AsNoTracking()
            .Where(b => keys.Contains(b.OpenLibraryKey) || (b.Isbn != null && isbns.Contains(b.Isbn)))
            .Select(b => new { b.OpenLibraryKey, b.Isbn })
            .ToListAsync(cancellationToken);
        var ownedKeys = owned.Select(o => o.OpenLibraryKey).ToHashSet();
        var ownedIsbns = owned.Select(o => o.Isbn).OfType<string>().ToHashSet();

        var items = page.Items
            .Select(i => new CatalogBookDto(
                i.Key, i.Title, i.Author, i.Isbn, i.PublishedYear, i.CoverUrl,
                AlreadyInStore: ownedKeys.Contains(i.Key) || (Book.NormalizeIsbn(i.Isbn) is { } isbn && ownedIsbns.Contains(isbn))))
            .ToList();

        return new CatalogSearchResultDto(items, query.Page, query.PageSize, page.TotalCount);
    }
}
