using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookStore.Application.Abstractions.Catalog;
using BookStore.Application.Common;
using BookStore.Application.Features.Books;
using BookStore.Application.Features.Catalog.SearchCatalog;

namespace BookStore.Tests.Api;

/// <summary>End-to-end over HTTP: search the (fake) catalog, add the selection, see it listed, update, delete.</summary>
public sealed class BooksApiTests(BookStoreApiFactory factory) : IClassFixture<BookStoreApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Fact]
    public async Task Search_select_persist_list_update_delete()
    {
        var client = factory.CreateClient();
        factory.Catalog.Books.Add(new CatalogBook("/works/OL999W", "Kindred", "Octavia E. Butler", "9780807083697", 1979,
            "https://covers.openlibrary.org/b/id/1-M.jpg"));

        var search = await client.GetFromJsonAsync<CatalogSearchResultDto>("/api/catalog/search?q=kindred", Json);
        var pick = Assert.Single(search!.Items);
        Assert.False(pick.AlreadyInStore);

        var created = await client.PostAsJsonAsync("/api/books", new
        {
            pick.Title, pick.Author, pick.Isbn, pick.PublishedYear, pick.CoverUrl,
            OpenLibraryKey = pick.Key, Genre = "SciFi", Price = 14.99m, Stock = 2,
        }, Json);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var book = (await created.Content.ReadFromJsonAsync<BookDto>(Json))!;

        var duplicate = await client.PostAsJsonAsync("/api/books", new
        {
            OpenLibraryKey = pick.Key, pick.Title, pick.Author, Genre = "SciFi", Price = 1m, Stock = 0,
        }, Json);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var listed = await client.GetFromJsonAsync<PagedResult<BookDto>>("/api/books?genre=SciFi&pageSize=100", Json);
        Assert.Contains(listed!.Items, b => b.Id == book.Id);

        var updated = await client.PutAsJsonAsync($"/api/books/{book.Id}", new { Genre = "History", Price = 20m, Stock = 5 }, Json);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/books/{book.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/books/{book.Id}")).StatusCode);
    }

    [Fact]
    public async Task Invalid_input_returns_validation_problem_details()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/books", new
        {
            OpenLibraryKey = "/works/OL1W", Title = "", Author = "A", Genre = "SciFi", Price = 0m, Stock = -1,
        }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("title", out _));
        Assert.True(errors.TryGetProperty("price", out _));
        Assert.True(errors.TryGetProperty("stock", out _));
    }

    [Fact]
    public async Task Catalog_outage_returns_503_with_a_correlation_id()
    {
        factory.Catalog.IsDown = true;
        try
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Correlation-ID", "trace-me");

            var response = await client.GetAsync("/api/catalog/search?q=anything");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("trace-me", response.Headers.GetValues("X-Correlation-ID").Single());
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Catalog.Unavailable", problem.GetProperty("code").GetString());
        }
        finally
        {
            factory.Catalog.IsDown = false;
        }
    }
}
