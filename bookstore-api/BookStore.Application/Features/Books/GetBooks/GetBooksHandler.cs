using System.Linq.Expressions;
using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Abstractions.Persistence;
using BookStore.Application.Common;
using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books.GetBooks;

internal sealed class GetBooksHandler(IBookStoreDbContext db) : IRequestHandler<GetBooksQuery, PagedResult<BookDto>>
{
    public async Task<PagedResult<BookDto>> Handle(GetBooksQuery query, CancellationToken cancellationToken)
    {
        var books = db.Books.AsNoTracking();
        if (query.Genre is { } genre) books = books.Where(b => b.Genre == genre);

        var totalCount = await books.CountAsync(cancellationToken);

        var page = await ApplySort(books, query.SortBy, query.SortDirection)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BookDto>(page.Select(BookDto.From).ToList(), query.Page, query.PageSize, totalCount);
    }

    private static IQueryable<Book> ApplySort(IQueryable<Book> books, BookSortField field, SortDirection direction) =>
        field switch
        {
            BookSortField.Author => OrderBy(books, b => b.Author, direction),
            BookSortField.PublishedYear => OrderBy(books, b => b.PublishedYear, direction),
            BookSortField.Genre => OrderBy(books, b => b.Genre, direction),
            BookSortField.Price => OrderBy(books, b => b.Price, direction),
            BookSortField.Stock => OrderBy(books, b => b.Stock, direction),
            BookSortField.CreatedAt => OrderBy(books, b => b.CreatedAt, direction),
            _ => OrderBy(books, b => b.Title, direction),
        };

    // Id as a tie-breaker keeps paging stable when many rows share a sort key.
    private static IQueryable<Book> OrderBy<TKey>(IQueryable<Book> books, Expression<Func<Book, TKey>> key, SortDirection direction) =>
        direction == SortDirection.Desc
            ? books.OrderByDescending(key).ThenBy(b => b.Id)
            : books.OrderBy(key).ThenBy(b => b.Id);
}
