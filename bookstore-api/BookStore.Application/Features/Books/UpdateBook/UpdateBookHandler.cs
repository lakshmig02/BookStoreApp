using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Abstractions.Persistence;
using BookStore.Application.Common;

namespace BookStore.Application.Features.Books.UpdateBook;

internal sealed class UpdateBookHandler(IBookStoreDbContext db, TimeProvider clock)
    : IRequestHandler<UpdateBookCommand, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        var book = await db.Books.FindAsync([command.Id], cancellationToken);
        if (book is null) return BookErrors.NotFound(command.Id);

        book.UpdateStoreDetails(command.Genre, command.Price, command.Stock, command.Description, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        return BookDto.From(book);
    }
}
