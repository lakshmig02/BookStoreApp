using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Infrastructure.Persistence;

/// <summary>Seeds a handful of books (one or more per genre) on first run so the grid isn't empty.</summary>
internal sealed partial class BookStoreSeeder(BookStoreDbContext db, TimeProvider clock, ILogger<BookStoreSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Books.AnyAsync(cancellationToken)) return;

        var now = clock.GetUtcNow();
        Book Make(string title, string author, string isbn, int year, string key, Genre genre, decimal price, int stock, string? description = null) =>
            Book.Create(title, author, isbn, year, $"https://covers.openlibrary.org/b/isbn/{isbn}-M.jpg", key, description, genre, price, stock, now);

        Book[] books =
        [
            Make("Dune", "Frank Herbert", "9780441172719", 1965, "/works/OL893415W", Genre.SciFi, 10.99m, 12,
                "A desert planet, a precious spice, and a young heir caught in an interstellar power struggle."),
            Make("Foundation", "Isaac Asimov", "9780553293357", 1951, "/works/OL46125W", Genre.SciFi, 8.99m, 7),
            Make("The Hobbit", "J.R.R. Tolkien", "9780547928227", 1937, "/works/OL262758W", Genre.Fantasy, 12.50m, 20),
            Make("The Lord of the Rings", "J.R.R. Tolkien", "9780544003415", 1954, "/works/OL27448W", Genre.Fantasy, 24.00m, 5),
            Make("Pride and Prejudice", "Jane Austen", "9780141439518", 1813, "/works/OL66554W", Genre.Fiction, 7.99m, 9),
            Make("Nineteen Eighty-Four", "George Orwell", "9780451524935", 1949, "/works/OL1168083W", Genre.Fiction, 9.99m, 14),
            Make("Murder on the Orient Express", "Agatha Christie", "9780062693662", 1934, "/works/OL471576W", Genre.Mystery, 11.25m, 6),
            Make("Steve Jobs", "Walter Isaacson", "9781451648539", 2011, "/works/OL16239864W", Genre.Biography, 18.00m, 3),
            Make("Sapiens", "Yuval Noah Harari", "9780062316097", 2011, "/works/OL17075811W", Genre.NonFiction, 16.99m, 11),
            Make("Charlotte's Web", "E. B. White", "9780064400558", 1952, "/works/OL45883W", Genre.Children, 6.99m, 15),
            Make("The Guns of August", "Barbara W. Tuchman", "9780345476098", 1962, "/works/OL1924997W", Genre.History, 14.50m, 4),
        ];

        db.Books.AddRange(books);
        await db.SaveChangesAsync(cancellationToken);
        LogSeeded(books.Length);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded {BookCount} sample books")]
    private partial void LogSeeded(int bookCount);
}
