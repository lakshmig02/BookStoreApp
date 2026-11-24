using BookStore.Domain.Books;
using FluentValidation;

namespace BookStore.Application.Features.Books;

/// <summary>Validation rules for the store-owned fields, shared by AddBook and UpdateBook.</summary>
internal static class BookRules
{
    public static IRuleBuilderOptions<T, decimal> ValidPrice<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.GreaterThan(0).LessThanOrEqualTo(Book.MaxPrice).PrecisionScale(8, 2, ignoreTrailingZeros: true);

    public static IRuleBuilderOptions<T, int> ValidStock<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(0, 1_000_000);

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(Book.DescriptionMaxLength);
}
