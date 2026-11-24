using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Common;
using BookStore.Domain.Books;

namespace BookStore.Application.Features.Books.GetBooks;

public enum BookSortField
{
    Title,
    Author,
    PublishedYear,
    Genre,
    Price,
    Stock,
    CreatedAt,
}

public enum SortDirection
{
    Asc,
    Desc,
}

public sealed record GetBooksQuery(
    Genre? Genre = null,
    int Page = 1,
    int PageSize = 10,
    BookSortField SortBy = BookSortField.Title,
    SortDirection SortDirection = SortDirection.Asc) : IQuery<PagedResult<BookDto>>
{
    public const int MaxPageSize = 100;
}
