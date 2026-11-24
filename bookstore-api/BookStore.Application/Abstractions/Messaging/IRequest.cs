namespace BookStore.Application.Abstractions.Messaging;

/// <summary>A message dispatched through the mediator that produces <typeparamref name="TResponse"/>.</summary>
public interface IRequest<out TResponse>;

/// <summary>A request that changes state. Commands are the only requests allowed to write.</summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;

/// <summary>A side-effect-free read.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
