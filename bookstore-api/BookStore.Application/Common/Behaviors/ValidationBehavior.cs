using BookStore.Application.Abstractions.Messaging;
using FluentValidation;

namespace BookStore.Application.Common.Behaviors;

/// <summary>
/// Runs every FluentValidation validator registered for the request before the handler.
/// Invalid input is a client bug, not a business outcome, so it short-circuits with a
/// <see cref="ValidationException"/> that the API maps to a 400 problem response.
/// </summary>
internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).ToList();
            if (failures.Count > 0) throw new ValidationException(failures);
        }

        return await next();
    }
}
