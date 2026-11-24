using System.Reflection;

namespace BookStore.Tests.Architecture;

/// <summary>Guards the Clean Architecture rule: dependencies only point inward.</summary>
public sealed class LayerDependencyTests
{
    private static readonly Assembly Domain = typeof(BookStore.Domain.Books.Book).Assembly;
    private static readonly Assembly Application = typeof(BookStore.Application.DependencyInjection).Assembly;
    private static readonly Assembly Infrastructure = typeof(BookStore.Infrastructure.DependencyInjection).Assembly;

    private static IEnumerable<string> References(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name!);

    [Fact]
    public void Domain_depends_on_nothing_but_the_runtime() =>
        Assert.DoesNotContain(References(Domain), name =>
            name.StartsWith("BookStore.") || name.StartsWith("Microsoft.EntityFrameworkCore") || name.StartsWith("Microsoft.AspNetCore"));

    [Fact]
    public void Application_does_not_depend_on_infrastructure_web_or_a_database_provider() =>
        Assert.DoesNotContain(References(Application), name =>
            name is "BookStore.Infrastructure" or "BookStore.Api"
            || name.StartsWith("Microsoft.AspNetCore")
            || name.StartsWith("Microsoft.EntityFrameworkCore.Sqlite")
            || name.StartsWith("System.Net.Http"));

    [Fact]
    public void Infrastructure_does_not_depend_on_the_api() =>
        Assert.DoesNotContain("BookStore.Api", References(Infrastructure));
}
