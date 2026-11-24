using BookStore.Application.Common;
using BookStore.Application.Features.Books.AddBook;
using BookStore.Domain.Books;
using BookStore.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Tests.Application;

public sealed class AddBookHandlerTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    private static AddBookCommand Command(string key = "/works/OL45804W", string? isbn = "9780441172719") =>
        new(key, "Dune", "Frank Herbert", isbn, 1965, "https://covers.openlibrary.org/b/id/1-M.jpg",
            null, Genre.SciFi, 9.99m, 4);

    private async Task<Result<BookStore.Application.Features.Books.BookDto>> Send(AddBookCommand command)
    {
        await using var db = _database.CreateContext();
        return await new AddBookHandler(db, TimeProvider.System).Handle(command, CancellationToken.None);
    }

    [Fact]
    public async Task Persists_catalog_fields_together_with_store_fields()
    {
        var result = await Send(Command());

        Assert.True(result.IsSuccess);
        await using var db = _database.CreateContext();
        var stored = await db.Books.SingleAsync();
        Assert.Equal(("Dune", "/works/OL45804W", "9780441172719"), (stored.Title, stored.OpenLibraryKey, stored.Isbn));
        Assert.Equal((Genre.SciFi, 9.99m, 4), (stored.Genre, stored.Price, stored.Stock));
    }

    [Fact]
    public async Task Rejects_a_second_book_with_the_same_open_library_key()
    {
        await Send(Command());

        var result = await Send(Command(isbn: null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Rejects_a_different_work_with_the_same_isbn_regardless_of_formatting()
    {
        await Send(Command());

        var result = await Send(Command(key: "/works/OL1W", isbn: "978-0-441-17271-9"));

        Assert.Equal("Book.Duplicate", result.Error?.Code);
        await using var db = _database.CreateContext();
        Assert.Equal(1, await db.Books.CountAsync());
    }
}
