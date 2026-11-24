using BookStore.Domain.Books;
using BookStore.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Tests.Support;

/// <summary>
/// A real (in-memory) SQLite database with the production migrations applied, so tests exercise
/// the same SQL, converters and unique indexes as the running app.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();
        using var db = CreateContext();
        db.Database.Migrate();
    }

    public BookStoreDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<BookStoreDbContext>().UseSqlite(_connection).Options);

    public async Task<Book> SeedAsync(
        string title,
        Genre genre = Genre.Fiction,
        decimal price = 10m,
        int stock = 1,
        string? isbn = null,
        string? key = null)
    {
        await using var db = CreateContext();
        var book = Book.Create(
            title, "Some Author", isbn, 2000, null,
            key ?? $"/works/OL{Random.Shared.Next(1, int.MaxValue)}W",
            null, genre, price, stock, DateTimeOffset.UtcNow);
        db.Books.Add(book);
        await db.SaveChangesAsync();
        return book;
    }

    public void Dispose() => _connection.Dispose();
}
