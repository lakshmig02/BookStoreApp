using BookStore.Application.Features.Books.GetBooks;
using BookStore.Domain.Books;
using BookStore.Tests.Support;

namespace BookStore.Tests.Application;

public sealed class GetBooksHandlerTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Filters_by_genre_then_sorts_and_pages_in_the_database()
    {
        await _database.SeedAsync("Cheap SciFi", Genre.SciFi, price: 5m);
        await _database.SeedAsync("Mid SciFi", Genre.SciFi, price: 15m);
        await _database.SeedAsync("Pricey SciFi", Genre.SciFi, price: 25m);
        await _database.SeedAsync("A Mystery", Genre.Mystery, price: 99m);

        await using var db = _database.CreateContext();
        var page = await new GetBooksHandler(db).Handle(
            new GetBooksQuery(Genre.SciFi, Page: 1, PageSize: 2, BookSortField.Price, SortDirection.Desc),
            CancellationToken.None);

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(["Pricey SciFi", "Mid SciFi"], page.Items.Select(b => b.Title));
    }

    [Fact]
    public async Task Without_a_genre_returns_every_book()
    {
        await _database.SeedAsync("One", Genre.SciFi);
        await _database.SeedAsync("Two", Genre.History);

        await using var db = _database.CreateContext();
        var page = await new GetBooksHandler(db).Handle(new GetBooksQuery(), CancellationToken.None);

        Assert.Equal(["One", "Two"], page.Items.Select(b => b.Title));
    }
}
