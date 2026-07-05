using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Transport SPI for consuming messages from a single subscription. Created by <see cref="ITransportConsumerFactory"/>
/// and driven by the shared consumption pipeline, which supplies the per-delivery callback and, after the handler has
/// run, calls back to acknowledge (<see cref="CompleteAsync"/>) or abandon (<see cref="AbandonAsync"/>) the delivery.
/// Disposing stops delivery and releases broker resources.
/// </summary>
public interface ITransportConsumer : IAsyncDisposable
{
    /// <summary>
    /// Starts delivering messages, invoking <paramref name="onMessage"/> for each one.
    /// </summary>
    /// <param name="onMessage">The callback invoked per received delivery.</param>
    /// <param name="ct">A token to cancel startup.</param>
    /// <returns>A task that completes once consumption has started.</returns>
    Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct);

    /// <summary>
    /// Acknowledges/commits the delivery at the transport level, marking it as successfully consumed.
    /// </summary>
    /// <param name="delivery">The delivery to acknowledge.</param>
    /// <returns>A task that completes when the acknowledgement is recorded.</returns>
    Task CompleteAsync(TransportDelivery delivery);

    /// <summary>
    /// Abandons the delivery. Under at-least-once the transport redelivers it (raw redelivery); under at-most-once it
    /// is dropped. Used when the handler faults without an explicit disposition; the retry policy is not engaged.
    /// </summary>
    /// <param name="delivery">The delivery to abandon.</param>
    /// <returns>A task that completes when the abandonment is recorded.</returns>
    Task AbandonAsync(TransportDelivery delivery);
}
