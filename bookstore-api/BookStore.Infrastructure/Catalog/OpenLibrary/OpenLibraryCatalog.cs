using System.Diagnostics;
using System.Net.Http.Json;
using BookStore.Application.Abstractions.Catalog;
using BookStore.Domain.Books;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookStore.Infrastructure.Catalog.OpenLibrary;

/// <summary>
/// Adapter (anti-corruption layer) from Open Library's search API to <see cref="IBookCatalog"/>.
/// Timeouts and retries come from the resilience pipeline configured on the typed HttpClient;
/// every upstream failure is translated into <see cref="CatalogUnavailableException"/>.
/// </summary>
internal sealed partial class OpenLibraryCatalog(
    HttpClient http,
    IOptions<OpenLibraryOptions> options,
    ILogger<OpenLibraryCatalog> logger) : IBookCatalog
{
    private const string Fields = "key,title,author_name,first_publish_year,isbn,cover_i";

    public async Task<CatalogSearchPage> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var uri = BuildSearchUri(query, page, pageSize);
        var started = Stopwatch.GetTimestamp();

        OpenLibrarySearchResponse? response;
        try
        {
            response = await http.GetFromJsonAsync<OpenLibrarySearchResponse>(uri, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // The caller went away; not an upstream failure.
        }
        catch (Exception ex)
        {
            LogSearchFailed(ex, query, Stopwatch.GetElapsedTime(started).TotalMilliseconds, ex.GetType().Name);
            throw new CatalogUnavailableException("Open Library search failed.", ex);
        }

        var items = (response?.Docs ?? [])
            .Select(Normalize)
            .OfType<CatalogBook>()
            .ToList();

        LogSearchCompleted(query, items.Count, response?.NumFound ?? 0, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        return new CatalogSearchPage(items, response?.NumFound ?? 0);
    }

    /// <summary>ISBN-looking input is sent as an exact <c>isbn=</c> search; anything else as free text.</summary>
    internal static string BuildSearchUri(string query, int page, int pageSize)
    {
        var isbn = Book.NormalizeIsbn(query);
        var looksLikeIsbn = isbn?.Length is 10 or 13 && query.All(c => char.IsDigit(c) || c is '-' or ' ' or 'X' or 'x');
        var criteria = looksLikeIsbn ? $"isbn={isbn}" : $"q={Uri.EscapeDataString(query)}";
        return $"search.json?{criteria}&fields={Fields}&limit={pageSize}&page={page}";
    }

    internal CatalogBook? Normalize(OpenLibrarySearchDoc doc)
    {
        if (string.IsNullOrWhiteSpace(doc.Key) || string.IsNullOrWhiteSpace(doc.Title)) return null;

        var authors = doc.AuthorName?.Where(a => !string.IsNullOrWhiteSpace(a)).ToList() ?? [];
        var isbn = Book.NormalizeIsbn(doc.Isbn?.FirstOrDefault());

        return new CatalogBook(
            Key: doc.Key,
            Title: doc.Title.Trim(),
            Author: authors.Count > 0 ? string.Join(", ", authors) : "Unknown",
            Isbn: isbn,
            PublishedYear: doc.FirstPublishYear,
            CoverUrl: BuildCoverUrl(doc.CoverId, isbn));
    }

    // We only derive a URL; the image itself is served to the browser by Open Library's CDN.
    private string? BuildCoverUrl(long? coverId, string? isbn)
    {
        var baseUrl = options.Value.CoversBaseUrl.TrimEnd('/');
        if (coverId is > 0) return $"{baseUrl}/b/id/{coverId}-M.jpg";
        if (isbn is not null) return $"{baseUrl}/b/isbn/{isbn}-M.jpg";
        return null;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Open Library search for {CatalogQuery} returned {ResultCount} of {TotalCount} results in {ElapsedMs:0} ms")]
    private partial void LogSearchCompleted(string catalogQuery, int resultCount, int totalCount, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Open Library search for {CatalogQuery} failed after {ElapsedMs:0} ms ({FailureType})")]
    private partial void LogSearchFailed(Exception exception, string catalogQuery, double elapsedMs, string failureType);
}
