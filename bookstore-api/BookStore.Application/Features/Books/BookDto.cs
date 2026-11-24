using BookStore.Domain.Books;

namespace BookStore.Application.Features.Books;

/// <summary>Read model returned by every book slice. Shared within the Books feature only.</summary>
public sealed record BookDto(
    Guid Id,
    string Title,
    string Author,
    string? Isbn,
    int? PublishedYear,
    string? CoverUrl,
    string OpenLibraryKey,
    string? Description,
    Genre Genre,
    decimal Price,
    int Stock,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static BookDto From(Book book) => new(
        book.Id, book.Title, book.Author, book.Isbn, book.PublishedYear, book.CoverUrl,
        book.OpenLibraryKey, book.Description, book.Genre, book.Price, book.Stock,
        book.CreatedAt, book.UpdatedAt);
}
