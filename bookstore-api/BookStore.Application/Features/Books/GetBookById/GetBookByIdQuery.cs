using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Abstractions.Persistence;
using BookStore.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books.GetBookById;

public sealed record GetBookByIdQuery(Guid Id) : IQuery<Result<BookDto>>;

internal sealed class GetBookByIdHandler(IBookStoreDbContext db) : IRequestHandler<GetBookByIdQuery, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
    {
        var book = await db.Books.AsNoTracking().SingleOrDefaultAsync(b => b.Id == query.Id, cancellationToken);
        return book is null ? BookErrors.NotFound(query.Id) : BookDto.From(book);
    }
}
