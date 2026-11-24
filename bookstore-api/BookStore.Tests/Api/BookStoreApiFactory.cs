using BookStore.Application.Abstractions.Catalog;
using BookStore.Tests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookStore.Tests.Api;

/// <summary>Boots the real API against a throwaway SQLite file and a fake catalog (no network).</summary>
public sealed class BookStoreApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"bookstore-tests-{Guid.NewGuid():N}.db");

    internal FakeBookCatalog Catalog { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:BookStore", $"Data Source={_databasePath};Pooling=false");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBookCatalog>();
            services.AddSingleton<IBookCatalog>(Catalog);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        File.Delete(_databasePath);
    }
}
