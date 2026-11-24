using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Common;
using BookStore.Domain.Books;

namespace BookStore.Application.Features.Books.UpdateBook;

/// <summary>Edits store-owned fields only. Never touches Open Library.</summary>
public sealed record UpdateBookCommand(Guid Id, Genre Genre, decimal Price, int Stock, string? Description)
    : ICommand<Result<BookDto>>;
