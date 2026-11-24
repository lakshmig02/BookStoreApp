using BookStore.Api.Http;
using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Common;
using BookStore.Application.Features.Books;
using BookStore.Application.Features.Books.AddBook;
using BookStore.Application.Features.Books.DeleteBook;
using BookStore.Application.Features.Books.GetBookById;
using BookStore.Application.Features.Books.GetBooks;
using BookStore.Application.Features.Books.UpdateBook;
using BookStore.Domain.Books;

namespace BookStore.Api.Endpoints;

/// <summary>Store CRUD. None of these endpoints touch Open Library — the store owns these records.</summary>
internal static class BooksEndpoints
{
    public static IEndpointRouteBuilder MapBooksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", GetBooks)
            .WithName("GetBooks")
            .WithSummary("List store books with genre filter, paging and sorting")
            .Produces<PagedResult<BookDto>>()
            .ProducesValidationProblem();
        group.MapGet("/{id:guid}", GetBookById)
            .WithName("GetBookById")
            .WithSummary("Get one store book")
            .Produces<BookDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", AddBook)
            .WithName("AddBook")
            .WithSummary("Add a selected Open Library result to the store")
            .Produces<BookDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:guid}", UpdateBook)
            .WithName("UpdateBook")
            .WithSummary("Update store-owned fields (genre, price, stock, description)")
            .Produces<BookDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapDelete("/{id:guid}", DeleteBook)
            .WithName("DeleteBook")
            .WithSummary("Remove a book from the store")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    internal sealed record GetBooksParameters(
        Genre? Genre,
        int? Page,
        int? PageSize,
        BookSortField? SortBy,
        SortDirection? SortDir);

    public sealed record UpdateBookRequest(Genre Genre, decimal Price, int Stock, string? Description);

    private static async Task<IResult> GetBooks([AsParameters] GetBooksParameters p, ISender sender, CancellationToken ct)
    {
        var query = new GetBooksQuery(
            p.Genre,
            p.Page ?? 1,
            p.PageSize ?? 10,
            p.SortBy ?? BookSortField.Title,
            p.SortDir ?? SortDirection.Asc);
        return TypedResults.Ok(await sender.Send(query, ct));
    }

    private static async Task<IResult> GetBookById(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetBookByIdQuery(id), ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    private static async Task<IResult> AddBook(AddBookCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetBookById", new { id = result.Value.Id })
            : result.Error.ToProblem();
    }

    private static async Task<IResult> UpdateBook(Guid id, UpdateBookRequest request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateBookCommand(id, request.Genre, request.Price, request.Stock, request.Description), ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    private static async Task<IResult> DeleteBook(Guid id, ISender sender, CancellationToken ct)
    {
        Result result = await sender.Send(new DeleteBookCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }
}
