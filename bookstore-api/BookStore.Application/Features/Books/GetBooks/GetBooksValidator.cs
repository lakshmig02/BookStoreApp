using FluentValidation;

namespace BookStore.Application.Features.Books.GetBooks;

internal sealed class GetBooksValidator : AbstractValidator<GetBooksQuery>
{
    public GetBooksValidator()
    {
        RuleFor(x => x.Genre).IsInEnum();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetBooksQuery.MaxPageSize);
        RuleFor(x => x.SortBy).IsInEnum();
        RuleFor(x => x.SortDirection).IsInEnum();
    }
}
