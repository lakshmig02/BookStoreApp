using BookStore.Application.Abstractions.Catalog;
using BookStore.Application.Common;
using BookStore.Application.Features.Catalog.SearchCatalog;
using BookStore.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Tests.Application;

public sealed class SearchCatalogHandlerTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly FakeBookCatalog _catalog = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Flags_results_already_in_the_store_by_key_or_isbn_and_persists_nothing()
    {
        await _database.SeedAsync("Owned by key", key: "/works/OL1W");
        await _database.SeedAsync("Owned by isbn", isbn: "9780000000002");
        _catalog.Books.AddRange(
        [
            new CatalogBook("/works/OL1W", "Owned by key", "A", null, null, null),
            new CatalogBook("/works/OL2W", "Owned by isbn", "A", "978-0-00-000000-2", null, null),
            new CatalogBook("/works/OL3W", "New", "A", "9780000000003", null, null),
        ]);

        await using var db = _database.CreateContext();
        var result = await new SearchCatalogHandler(_catalog, db).Handle(new SearchCatalogQuery("x"), CancellationToken.None);

        Assert.Equal([true, true, false], result.Value.Items.Select(i => i.AlreadyInStore));
        Assert.Equal(2, await db.Books.CountAsync()); // search results are transient
    }

    [Fact]
    public async Task Maps_catalog_outage_to_an_unavailable_error()
    {
        _catalog.IsDown = true;

        await using var db = _database.CreateContext();
        var result = await new SearchCatalogHandler(_catalog, db).Handle(new SearchCatalogQuery("dune"), CancellationToken.None);

        Assert.Equal(ErrorType.Unavailable, result.Error?.Type);
    }
}
