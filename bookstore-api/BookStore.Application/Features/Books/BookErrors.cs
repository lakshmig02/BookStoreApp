using BookStore.Application.Common;

namespace BookStore.Application.Features.Books;

public static class BookErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Book.NotFound", $"Book '{id}' was not found in the store.");

    public static Error Duplicate(string title) =>
        Error.Conflict("Book.Duplicate", $"'{title}' is already in the store (same Open Library key or ISBN).");
}
