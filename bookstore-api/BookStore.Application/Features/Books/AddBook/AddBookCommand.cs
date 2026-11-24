using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Common;
using BookStore.Domain.Books;

namespace BookStore.Application.Features.Books.AddBook;

/// <summary>
/// Persists a catalog result the user selected, combined with the store-owned fields.
/// The bibliographic fields are those returned by SearchCatalog; they are copied once and owned from then on.
/// </summary>
public sealed record AddBookCommand(
    string OpenLibraryKey,
    string Title,
    string Author,
    string? Isbn,
    int? PublishedYear,
    string? CoverUrl,
    string? Description,
    Genre Genre,
    decimal Price,
    int Stock) : ICommand<Result<BookDto>>;
