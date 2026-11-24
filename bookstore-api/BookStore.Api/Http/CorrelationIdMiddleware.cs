using Serilog.Context;

namespace BookStore.Api.Http;

/// <summary>
/// Accepts an inbound <c>X-Correlation-ID</c> (or generates one), echoes it on the response and
/// pushes it into the Serilog LogContext so every log line of the request — including the
/// outbound Open Library call — carries the same <c>CorrelationId</c> property.
/// </summary>
internal sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";
    private const int MaxLength = 64;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ReadValidHeader(context) ?? Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    // Untrusted input ends up in logs and headers: only accept short, plain tokens.
    private static string? ReadValidHeader(HttpContext context)
    {
        var value = context.Request.Headers[HeaderName].ToString();
        return value.Length is > 0 and <= MaxLength && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
            ? value
            : null;
    }
}
