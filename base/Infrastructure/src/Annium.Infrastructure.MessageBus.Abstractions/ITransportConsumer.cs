using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// Transport SPI for consuming messages from a single subscription. Created by <see cref="ITransportConsumerFactory"/>
/// and driven by the shared consumption pipeline, which supplies the per-message callback. Disposing stops delivery
/// and releases broker resources.
/// </summary>
public interface ITransportConsumer : IAsyncDisposable
{
    /// <summary>
    /// Starts delivering messages, invoking <paramref name="onMessage"/> for each one.
    /// </summary>
    /// <param name="onMessage">The callback invoked per received message.</param>
    /// <param name="ct">A token to cancel startup.</param>
    /// <returns>A task that completes once consumption has started.</returns>
    Task StartAsync(Func<ITransportIncomingMessage, CancellationToken, Task> onMessage, CancellationToken ct);
}
