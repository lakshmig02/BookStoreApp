using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Abstractions.Persistence;
using BookStore.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books.DeleteBook;

public sealed record DeleteBookCommand(Guid Id) : ICommand<Result>;

internal sealed class DeleteBookHandler(IBookStoreDbContext db) : IRequestHandler<DeleteBookCommand, Result>
{
    public async Task<Result> Handle(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        var deleted = await db.Books.Where(b => b.Id == command.Id).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? BookErrors.NotFound(command.Id) : Result.Success();
    }
}
