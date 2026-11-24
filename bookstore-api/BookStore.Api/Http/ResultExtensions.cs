using BookStore.Application.Common;

namespace BookStore.Api.Http;

internal static class ResultExtensions
{
    /// <summary>Maps an expected use-case failure to a problem response with a stable error code.</summary>
    public static IResult ToProblem(this Error error) => TypedResults.Problem(
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status400BadRequest,
        },
        title: error.Message,
        extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
