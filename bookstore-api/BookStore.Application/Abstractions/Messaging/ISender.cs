namespace BookStore.Application.Abstractions.Messaging;

/// <summary>Entry point used by the API to dispatch commands and queries to their handlers.</summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
