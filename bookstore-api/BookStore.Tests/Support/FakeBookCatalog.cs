using BookStore.Application.Abstractions.Catalog;

namespace BookStore.Tests.Support;

internal sealed class FakeBookCatalog : IBookCatalog
{
    public List<CatalogBook> Books { get; } = [];
    public bool IsDown { get; set; }
    public int Calls { get; private set; }

    public Task<CatalogSearchPage> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken)
    {
        Calls++;
        if (IsDown) throw new CatalogUnavailableException("down");
        return Task.FromResult(new CatalogSearchPage(Books.Take(pageSize).ToList(), Books.Count));
    }
}
