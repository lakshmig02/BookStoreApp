using System.Text.Json.Serialization;

namespace BookStore.Infrastructure.Catalog.OpenLibrary;

/// <summary>Wire format of https://openlibrary.org/search.json — never leaves this folder.</summary>
internal sealed record OpenLibrarySearchResponse(
    [property: JsonPropertyName("numFound")] int NumFound,
    [property: JsonPropertyName("docs")] IReadOnlyList<OpenLibrarySearchDoc>? Docs);

internal sealed record OpenLibrarySearchDoc(
    [property: JsonPropertyName("key")] string? Key,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("author_name")] IReadOnlyList<string>? AuthorName,
    [property: JsonPropertyName("first_publish_year")] int? FirstPublishYear,
    [property: JsonPropertyName("isbn")] IReadOnlyList<string>? Isbn,
    [property: JsonPropertyName("cover_i")] long? CoverId);
