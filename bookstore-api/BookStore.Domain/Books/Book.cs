using BookStore.Domain.Common;

namespace BookStore.Domain.Books;

/// <summary>
/// A book owned by the store. Bibliographic fields are captured from Open Library once,
/// at add-time; store fields (genre, price, stock) are owned and edited by the store.
/// </summary>
public sealed class Book
{
    public const int TitleMaxLength = 500;
    public const int AuthorMaxLength = 300;
    public const int IsbnMaxLength = 13;
    public const int OpenLibraryKeyMaxLength = 64;
    public const int CoverUrlMaxLength = 300;
    public const int DescriptionMaxLength = 4000;
    public const decimal MaxPrice = 100_000m;

    // Required by EF Core for materialization.
    private Book()
    {
        Title = null!;
        Author = null!;
        OpenLibraryKey = null!;
    }

    private Book(
        Guid id,
        string title,
        string author,
        string? isbn,
        int? publishedYear,
        string? coverUrl,
        string openLibraryKey,
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Author = author;
        Isbn = isbn;
        PublishedYear = publishedYear;
        CoverUrl = coverUrl;
        OpenLibraryKey = openLibraryKey;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    // --- Captured from Open Library at add-time (immutable afterwards) ---
    public string Title { get; private set; }
    public string Author { get; private set; }
    public string? Isbn { get; private set; }
    public int? PublishedYear { get; private set; }
    public string? CoverUrl { get; private set; }
    public string OpenLibraryKey { get; private set; }

    // --- Owned by the store ---
    public string? Description { get; private set; }
    public Genre Genre { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Book Create(
        string title,
        string author,
        string? isbn,
        int? publishedYear,
        string? coverUrl,
        string openLibraryKey,
        string? description,
        Genre genre,
        decimal price,
        int stock,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(openLibraryKey)) throw new DomainException("Open Library key is required.");

        var book = new Book(
            Guid.CreateVersion7(now),
            title.Trim(),
            string.IsNullOrWhiteSpace(author) ? "Unknown" : author.Trim(),
            NormalizeIsbn(isbn),
            publishedYear,
            string.IsNullOrWhiteSpace(coverUrl) ? null : coverUrl.Trim(),
            openLibraryKey.Trim(),
            now);

        book.ApplyStoreDetails(genre, price, stock, description);
        return book;
    }

    /// <summary>Updates the store-owned fields. Open Library fields are never re-fetched.</summary>
    public void UpdateStoreDetails(Genre genre, decimal price, int stock, string? description, DateTimeOffset now)
    {
        ApplyStoreDetails(genre, price, stock, description);
        UpdatedAt = now;
    }

    /// <summary>Strips hyphens/spaces so "978-0-441-17271-9" and "9780441172719" compare equal.</summary>
    public static string? NormalizeIsbn(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return null;
        var normalized = new string(isbn.Where(c => char.IsDigit(c) || c is 'X' or 'x').ToArray()).ToUpperInvariant();
        return normalized.Length == 0 ? null : normalized;
    }

    private void ApplyStoreDetails(Genre genre, decimal price, int stock, string? description)
    {
        if (!Enum.IsDefined(genre)) throw new DomainException($"Unknown genre '{genre}'.");
        if (price is < 0 or > MaxPrice) throw new DomainException("Price is out of range.");
        if (stock < 0) throw new DomainException("Stock cannot be negative.");

        Genre = genre;
        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        Stock = stock;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
