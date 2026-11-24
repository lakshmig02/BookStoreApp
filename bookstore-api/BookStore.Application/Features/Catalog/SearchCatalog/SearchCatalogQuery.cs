using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Common;

namespace BookStore.Application.Features.Catalog.SearchCatalog;

/// <summary>Searches the external catalog. Results are transient: nothing is persisted.</summary>
public sealed record SearchCatalogQuery(string Q, int Page = 1, int PageSize = 10) : IQuery<Result<CatalogSearchResultDto>>
{
    public const int MaxPageSize = 20;
}

public sealed record CatalogSearchResultDto(IReadOnlyList<CatalogBookDto> Items, int Page, int PageSize, int TotalCount);

/// <param name="AlreadyInStore">True when a store book shares this key or ISBN — lets the UI prevent duplicates up front.</param>
public sealed record CatalogBookDto(
    string Key,
    string Title,
    string Author,
    string? Isbn,
    int? PublishedYear,
    string? CoverUrl,
    bool AlreadyInStore);
