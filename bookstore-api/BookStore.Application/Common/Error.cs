namespace BookStore.Application.Common;

public enum ErrorType
{
    NotFound,
    Conflict,
    Unavailable,
}

/// <summary>An expected, non-exceptional failure outcome of a use case.</summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unavailable(string code, string message) => new(code, message, ErrorType.Unavailable);
}
