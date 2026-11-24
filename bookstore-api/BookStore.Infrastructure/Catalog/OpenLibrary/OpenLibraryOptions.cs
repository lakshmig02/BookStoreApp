using System.ComponentModel.DataAnnotations;

namespace BookStore.Infrastructure.Catalog.OpenLibrary;

public sealed class OpenLibraryOptions
{
    public const string SectionName = "OpenLibrary";

    [Required, Url] public string BaseUrl { get; init; } = "https://openlibrary.org/";
    [Required, Url] public string CoversBaseUrl { get; init; } = "https://covers.openlibrary.org/";

    /// <summary>Open Library asks API clients to identify themselves.</summary>
    [Required] public string UserAgent { get; init; } = "BookStore/1.0 (technical assessment)";

    /// <summary>Per-attempt timeout. Open Library search can be slow; we fail fast and retry once.</summary>
    [Range(1, 60)] public int AttemptTimeoutSeconds { get; init; } = 5;

    /// <summary>Upper bound for the whole call including retries.</summary>
    [Range(1, 120)] public int TotalTimeoutSeconds { get; init; } = 12;
}
