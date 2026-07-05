using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Transport SPI for producing messages. Implemented by each adapter; consumed by the shared publish pipeline
/// (and by the consumption pipeline for dead-letter publishing).
/// </summary>
public interface ITransportProducer
{
    /// <summary>
    /// Produces a single message to the transport.
    /// </summary>
    /// <param name="message">The message to produce.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the message has been handed to the transport.</returns>
    Task ProduceAsync(TransportMessage message, CancellationToken ct);

    /// <summary>
    /// Produces a batch of messages to the transport.
    /// </summary>
    /// <param name="messages">The messages to produce.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when all messages have been handed to the transport.</returns>
    Task ProduceBatchAsync(IReadOnlyCollection<TransportMessage> messages, CancellationToken ct);
}
