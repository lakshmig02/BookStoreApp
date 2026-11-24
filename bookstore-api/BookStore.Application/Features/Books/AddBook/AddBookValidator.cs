using BookStore.Domain.Books;
using FluentValidation;

namespace BookStore.Application.Features.Books.AddBook;

internal sealed class AddBookValidator : AbstractValidator<AddBookCommand>
{
    public AddBookValidator(TimeProvider clock)
    {
        RuleFor(x => x.OpenLibraryKey)
            .NotEmpty()
            .MaximumLength(Book.OpenLibraryKeyMaxLength)
            .Matches("^/(works|books)/OL[0-9]+[WM]$")
            .WithMessage("'Open Library Key' must look like '/works/OL45804W'.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Book.TitleMaxLength);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(Book.AuthorMaxLength);
        RuleFor(x => x.Isbn)
            .Must(isbn => Book.NormalizeIsbn(isbn)?.Length is null or 10 or 13)
            .WithMessage("'Isbn' must contain 10 or 13 characters.");
        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(0, clock.GetUtcNow().Year + 1)
            .When(x => x.PublishedYear is not null);
        RuleFor(x => x.CoverUrl)
            .MaximumLength(Book.CoverUrlMaxLength)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            .When(x => !string.IsNullOrEmpty(x.CoverUrl))
            .WithMessage("'Cover Url' must be an absolute https URL.");
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Genre).IsInEnum();
        RuleFor(x => x.Price).ValidPrice();
        RuleFor(x => x.Stock).ValidStock();
    }
}
