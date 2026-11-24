using BookStore.Application;
using BookStore.Application.Abstractions.Catalog;
using BookStore.Application.Abstractions.Messaging;
using BookStore.Application.Features.Catalog.SearchCatalog;
using BookStore.Infrastructure;
using BookStore.Tests.Support;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Tests.Application;

public sealed class MediatorPipelineTests
{
    [Fact]
    public async Task Invalid_requests_are_rejected_by_the_pipeline_before_reaching_the_handler()
    {
        var catalog = new FakeBookCatalog();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:BookStore"] = "Data Source=:memory:" })
            .Build();
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddInfrastructure(configuration, AppContext.BaseDirectory)
            .AddSingleton<IBookCatalog>(catalog)
            .BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var exception = await Assert.ThrowsAsync<ValidationException>(() => sender.Send(new SearchCatalogQuery(" ")));

        Assert.Contains(exception.Errors, e => e.PropertyName == "q");
        Assert.Equal(0, catalog.Calls);
    }
}
