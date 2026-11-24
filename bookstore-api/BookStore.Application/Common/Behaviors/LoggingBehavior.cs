using System.Diagnostics;
using BookStore.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace BookStore.Application.Common.Behaviors;

/// <summary>Logs every request with structured properties and its duration.</summary>
internal sealed partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var started = Stopwatch.GetTimestamp();
        LogHandling(requestName);

        var response = await next();

        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        if (response is Result { IsSuccess: false } failed)
            LogFailed(requestName, failed.Error.Code, failed.Error.Type, elapsedMs);
        else
            LogHandled(requestName, elapsedMs);

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private partial void LogHandling(string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs:0.0} ms")]
    private partial void LogHandled(string requestName, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{RequestName} failed with {ErrorCode} ({ErrorType}) in {ElapsedMs:0.0} ms")]
    private partial void LogFailed(string requestName, string errorCode, ErrorType errorType, double elapsedMs);
}
