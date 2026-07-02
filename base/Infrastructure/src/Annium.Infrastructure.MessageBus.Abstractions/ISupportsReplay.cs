using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// Implemented by subscribers whose transport supports replay (starting consumption from a chosen position).
/// Detect via <c>is</c>/<c>as</c>; transports without replay (e.g. RabbitMQ) do not implement it.
/// </summary>
public interface ISupportsReplay
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
