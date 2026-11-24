using System.Text.Json.Serialization;
using BookStore.Api.Endpoints;
using BookStore.Api.Http;
using BookStore.Application;
using BookStore.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

    builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true); 
    builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["correlationId"] = ctx.HttpContext.TraceIdentifier);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging(o => o.GetLevel = (ctx, _, ex) =>
        ex is not null || ctx.Response.StatusCode is >= 500 and not 503
            ? Serilog.Events.LogEventLevel.Error
            : ctx.Response.StatusCode == 503 ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information);
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        
        app.MapOpenApi();
        app.UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint("/openapi/v1.json", "Book Store API v1");
            o.DocumentTitle = "Book Store API";
            o.DefaultModelsExpandDepth(-1); 
        });
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
    }

    app.MapBooksEndpoints();
    app.MapCatalogEndpoints();
    app.MapGenresEndpoints();

    await app.Services.InitializeDatabaseAsync();
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Book Store API terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
