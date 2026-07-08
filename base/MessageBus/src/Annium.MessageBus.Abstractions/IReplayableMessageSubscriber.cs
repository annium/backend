using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// A message subscriber whose transport also supports replay (starting consumption from a chosen position). Extends
/// <see cref="IMessageSubscriber"/>, so injecting it grants both plain and replay subscribes; transports without replay
/// (e.g. RabbitMQ) do not register it. Can also be discovered opportunistically via <c>is</c>/<c>as</c> from an
/// <see cref="IMessageSubscriber"/>.
/// </summary>
public interface IReplayableMessageSubscriber : IMessageSubscriber
{
    /// <summary>
    /// Subscribes with a start position (replay). See <see cref="IMessageSubscriber.SubscribeAsync{T}"/> for
    /// consumption and acknowledgement semantics.
    /// </summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="options">The replay subscription settings, including the start position.</param>
    /// <param name="handler">The per-message handler.</param>
    /// <returns>A task yielding a disposable that stops the subscription (graceful drain on dispose).</returns>
    Task<IAsyncDisposable> SubscribeAsync<T>(
        ReplaySubscriptionOptions options,
        Func<IMessageContext<T>, CancellationToken, Task> handler
    )
        where T : notnull;
}
