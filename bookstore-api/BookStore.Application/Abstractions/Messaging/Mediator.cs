using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Application.Abstractions.Messaging;

/// <summary>
/// Minimal hand-rolled mediator: resolves the single handler for a request plus any pipeline
/// behaviors, and composes them. Chosen over MediatR to avoid a commercial-license dependency
/// for ~60 lines of code we fully understand.
/// </summary>
internal sealed class Mediator(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, RequestDispatcher> Dispatchers = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dispatcher = (RequestDispatcher<TResponse>)Dispatchers.GetOrAdd(
            request.GetType(),
            static requestType => (RequestDispatcher)Activator.CreateInstance(
                typeof(RequestDispatcher<,>).MakeGenericType(requestType, typeof(TResponse)))!);

        return dispatcher.Dispatch(request, serviceProvider, cancellationToken);
    }

    private abstract class RequestDispatcher;

    private abstract class RequestDispatcher<TResponse> : RequestDispatcher
    {
        public abstract Task<TResponse> Dispatch(IRequest<TResponse> request, IServiceProvider services, CancellationToken ct);
    }

    private sealed class RequestDispatcher<TRequest, TResponse> : RequestDispatcher<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> Dispatch(IRequest<TResponse> request, IServiceProvider services, CancellationToken ct)
        {
            var typedRequest = (TRequest)request;
            var handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

            RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(typedRequest, ct);

            // Registration order = outermost first, so wrap in reverse.
            foreach (var behavior in services.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
            {
                var next = pipeline;
                pipeline = () => behavior.Handle(typedRequest, next, ct);
            }

            return pipeline();
        }
    }
}
