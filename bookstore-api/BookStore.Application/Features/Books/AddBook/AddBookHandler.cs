using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Abstractions.Persistence;
using BookStore.Application.Common;
using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books.AddBook;

internal sealed class AddBookHandler(IBookStoreDbContext db, TimeProvider clock)
    : IRequestHandler<AddBookCommand, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(AddBookCommand command, CancellationToken cancellationToken)
    {
        var book = Book.Create(
            command.Title, command.Author, command.Isbn, command.PublishedYear, command.CoverUrl,
            command.OpenLibraryKey, command.Description, command.Genre, command.Price, command.Stock,
            clock.GetUtcNow());

        var isDuplicate = await db.Books.AnyAsync(
            b => b.OpenLibraryKey == book.OpenLibraryKey || (book.Isbn != null && b.Isbn == book.Isbn),
            cancellationToken);
        if (isDuplicate) return BookErrors.Duplicate(book.Title);

        db.Books.Add(book);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Lost a race with a concurrent insert: the unique indexes are the final guard.
            return BookErrors.Duplicate(book.Title);
        }

        return BookDto.From(book);
    }
}
