using System.Net;
using System.Text;
using BookStore.Application.Abstractions.Catalog;
using BookStore.Infrastructure.Catalog.OpenLibrary;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BookStore.Tests.Infrastructure;

public sealed class OpenLibraryCatalogTests
{
    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }

    private static OpenLibraryCatalog CreateCatalog(StubHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("https://openlibrary.org/") },
        Options.Create(new OpenLibraryOptions()),
        NullLogger<OpenLibraryCatalog>.Instance);

    [Fact]
    public async Task Normalizes_search_docs_into_catalog_books()
    {
        const string json = """
            {
              "numFound": 42,
              "docs": [
                { "key": "/works/OL45804W", "title": "Fantastic Mr Fox", "author_name": ["Roald Dahl", "Quentin Blake"],
                  "first_publish_year": 1970, "isbn": ["9780140328721", "0140328726"], "cover_i": 6498519 },
                { "key": "/works/OL2W", "title": "No Cover", "isbn": ["0-14-032872-6"] },
                { "key": "/works/OL3W" }
              ]
            }
            """;
        var catalog = CreateCatalog(new StubHandler(HttpStatusCode.OK, json));

        var page = await catalog.SearchAsync("fox", 1, 10, CancellationToken.None);

        Assert.Equal(42, page.TotalCount);
        Assert.Equal(2, page.Items.Count); // the doc without a title is dropped
        Assert.Equal(
            new CatalogBook("/works/OL45804W", "Fantastic Mr Fox", "Roald Dahl, Quentin Blake", "9780140328721", 1970,
                "https://covers.openlibrary.org/b/id/6498519-M.jpg"),
            page.Items[0]);
        Assert.Equal(
            new CatalogBook("/works/OL2W", "No Cover", "Unknown", "0140328726", null,
                "https://covers.openlibrary.org/b/isbn/0140328726-M.jpg"),
            page.Items[1]);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "{}")]
    [InlineData(HttpStatusCode.OK, "<html>not json</html>")]
    public async Task Translates_upstream_failures_into_CatalogUnavailableException(HttpStatusCode status, string body)
    {
        var catalog = CreateCatalog(new StubHandler(status, body));

        await Assert.ThrowsAsync<CatalogUnavailableException>(() => catalog.SearchAsync("fox", 1, 10, CancellationToken.None));
    }

    [Theory]
    [InlineData("978-0-14-032872-1", "isbn=9780140328721")]
    [InlineData("the hobbit", "q=the%20hobbit")]
    [InlineData("1984", "q=1984")]
    public void Sends_isbn_looking_input_as_an_isbn_search(string query, string expectedCriteria)
    {
        var uri = OpenLibraryCatalog.BuildSearchUri(query, page: 2, pageSize: 5);

        Assert.StartsWith($"search.json?{expectedCriteria}&fields=", uri);
        Assert.EndsWith("&limit=5&page=2", uri);
    }
}
