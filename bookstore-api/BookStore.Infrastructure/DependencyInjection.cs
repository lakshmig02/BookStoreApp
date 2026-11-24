using BookStore.Application.Abstractions.Catalog;
using BookStore.Application.Abstractions.Persistence;
using BookStore.Application.Features.Genres.GetGenreSummary;
using BookStore.Infrastructure.Catalog.OpenLibrary;
using BookStore.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace BookStore.Infrastructure;

public static class DependencyInjection
{
    /// <param name="contentRootPath">Relative SQLite paths are resolved against this folder, not the CWD.</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        AddPersistence(services, configuration, contentRootPath);
        AddOpenLibrary(services, configuration);
        return services;
    }

    /// <summary>Applies pending migrations and seeds sample data. Called once at startup.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BookStoreDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<BookStoreSeeder>().SeedAsync(cancellationToken);
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        var connection = new SqliteConnectionStringBuilder(
            configuration.GetConnectionString("BookStore")
            ?? throw new InvalidOperationException("Connection string 'BookStore' is not configured."));
        if (connection.DataSource != ":memory:" && !Path.IsPathRooted(connection.DataSource))
            connection.DataSource = Path.Combine(contentRootPath, connection.DataSource);
        var connectionString = connection.ToString();

        services.AddDbContext<BookStoreDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IBookStoreDbContext>(sp => sp.GetRequiredService<BookStoreDbContext>());
        services.AddScoped<IGenreSummaryReader, GenreSummaryReader>();
        services.AddScoped<BookStoreSeeder>();
    }

    private static void AddOpenLibrary(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OpenLibraryOptions>()
            .Bind(configuration.GetSection(OpenLibraryOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IBookCatalog, OpenLibraryCatalog>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OpenLibraryOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.Timeout = Timeout.InfiniteTimeSpan; // The resilience pipeline owns timeouts.
            })
            .AddResilienceHandler("open-library", (pipeline, context) =>
            {
                var options = context.ServiceProvider.GetRequiredService<IOptions<OpenLibraryOptions>>().Value;
                var attempt = TimeSpan.FromSeconds(options.AttemptTimeoutSeconds);

                // Outer -> inner: total budget, a single polite retry, circuit breaker, per-attempt timeout.
                pipeline.AddTimeout(TimeSpan.FromSeconds(options.TotalTimeoutSeconds));
                pipeline.AddRetry(new HttpRetryStrategyOptions { MaxRetryAttempts = 1, Delay = TimeSpan.FromMilliseconds(300) });
                pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    SamplingDuration = attempt * 4,
                    MinimumThroughput = 5,
                    FailureRatio = 0.8,
                    BreakDuration = TimeSpan.FromSeconds(15),
                });
                pipeline.AddTimeout(attempt);
            });
    }
}
