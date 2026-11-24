using BookStore.Application.Common;
using BookStore.Application.Features.Books.UpdateBook;
using BookStore.Domain.Books;
using BookStore.Tests.Support;

namespace BookStore.Tests.Application;

public sealed class UpdateBookHandlerTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Updates_only_store_owned_fields()
    {
        var book = await _database.SeedAsync("Dune", Genre.Fiction, price: 10m, stock: 1);

        await using var db = _database.CreateContext();
        var result = await new UpdateBookHandler(db, TimeProvider.System).Handle(
            new UpdateBookCommand(book.Id, Genre.SciFi, 12.5m, 7, "  Spice.  "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((Genre.SciFi, 12.5m, 7, "Spice."), (result.Value.Genre, result.Value.Price, result.Value.Stock, result.Value.Description));
        Assert.Equal((book.Title, book.OpenLibraryKey), (result.Value.Title, result.Value.OpenLibraryKey));
    }

    [Fact]
    public async Task Returns_not_found_for_an_unknown_book()
    {
        await using var db = _database.CreateContext();
        var result = await new UpdateBookHandler(db, TimeProvider.System).Handle(
            new UpdateBookCommand(Guid.NewGuid(), Genre.SciFi, 1m, 0, null), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error?.Type);
    }
}
