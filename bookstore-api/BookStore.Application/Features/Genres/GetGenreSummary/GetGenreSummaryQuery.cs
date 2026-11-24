using BookStore.Application.Abstractions.Messaging;
using BookStore.Domain.Books;

namespace BookStore.Application.Features.Genres.GetGenreSummary;

/// <summary>Book count and stock per genre — powers the header genre filter.</summary>
public sealed record GetGenreSummaryQuery : IQuery<GenreSummaryDto>;

public sealed record GenreSummaryDto(int TotalBooks, IReadOnlyList<GenreCountDto> Genres);

public sealed record GenreCountDto(Genre Genre, int BookCount, int TotalStock);
