using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Subscribes to subjects and dispatches messages to a handler.
/// </summary>
public interface IMessageSubscriber
{
    /// <summary>
    /// Subscribes to a subject. The framework runs the consumption loop and invokes <paramref name="handler"/>
    /// per message (up to <see cref="SubscriptionOptions.Concurrency"/> concurrently). The handler must
    /// acknowledge each message via <see cref="IMessageContext{T}"/>.
    /// </summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="options">The subscription settings.</param>
    /// <param name="handler">The per-message handler.</param>
    /// <returns>A task yielding a disposable that stops the subscription (graceful drain on dispose).</returns>
    Task<IAsyncDisposable> SubscribeAsync<T>(
        SubscriptionOptions options,
        Func<IMessageContext<T>, CancellationToken, Task> handler
    )
        where T : notnull;
}
