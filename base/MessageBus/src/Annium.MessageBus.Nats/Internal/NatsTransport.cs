using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.MessageBus.Abstractions;

namespace Annium.MessageBus.Nats.Internal;

/// <summary>
/// The NATS transport: a producer plus a per-subscription consumer factory over the shared <see cref="NatsConnectionHolder"/>.
/// Registered as a singleton by <c>AddNatsMessageBus</c>.
/// </summary>
/// <remarks>
/// Produce always goes through JetStream (<c>js.PublishAsync</c>): the publish completes only once the stream has
/// acknowledged the write (zero-loss), and the canonical message id is mirrored to <c>Nats-Msg-Id</c> so the stream
/// deduplicates re-publishes. A JetStream stream capturing the subject must therefore be provisioned externally (this
/// adapter never creates one). Consumers are split by delivery mode: at-most-once uses a Core NATS subscription (no
/// acknowledgement, no redelivery), while at-least-once and replay use a JetStream pull consumer.
/// </remarks>
internal sealed class NatsTransport : ITransportProducer, ITransportConsumerFactory, ILogSubject
{
    /// <summary>
    /// The logger for this transport, passed on to created consumers.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The shared connection (Core + JetStream).
    /// </summary>
    private readonly NatsConnectionHolder _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="NatsTransport"/> class.
    /// </summary>
    /// <param name="connection">The shared connection.</param>
    /// <param name="logger">The logger passed to consumers.</param>
    public NatsTransport(NatsConnectionHolder connection, ILogger logger)
    {
        _connection = connection;
        Logger = logger;
    }

    /// <summary>
    /// Publishes a single message via JetStream, waiting for the stream's acknowledgement (zero-loss) and applying
    /// <c>Nats-Msg-Id</c> deduplication.
    /// </summary>
    /// <param name="message">The message to produce.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes once the stream has acknowledged the write.</returns>
    public async Task ProduceAsync(TransportMessage message, CancellationToken ct)
    {
        var jetStream = await _connection.GetJetStreamAsync(ct);
        var headers = NatsHeaderMapper.ToNatsHeaders(message.Headers);
        // JetStream publish waits for the stream's acknowledgement (zero-loss) and applies Nats-Msg-Id deduplication.
        await jetStream.PublishAsync(message.Subject, message.Body, headers: headers, cancellationToken: ct);
    }

    /// <summary>
    /// Publishes a batch of messages via JetStream by firing all <see cref="ProduceAsync"/> calls concurrently and
    /// awaiting them together, so each message's stream acknowledgement is awaited in parallel rather than serially.
    /// </summary>
    /// <param name="messages">The messages to produce.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes once every message's stream acknowledgement has been received.</returns>
    public async Task ProduceBatchAsync(IReadOnlyCollection<TransportMessage> messages, CancellationToken ct)
    {
        // Fire all publishes then await together; each awaits its own stream ack, so parallel avoids serializing the
        // batch into N sequential round-trips.
        var tasks = messages.Select(message => ProduceAsync(message, ct));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Creates a consumer bound to the given subscription options: a JetStream pull consumer for at-least-once or
    /// replay delivery (persistence, acknowledgement, positioned start), or a Core NATS subscription for plain
    /// at-most-once delivery (fire-and-forget, no redelivery).
    /// </summary>
    /// <param name="options">The subscription options.</param>
    /// <returns>A new transport consumer.</returns>
    public ITransportConsumer CreateConsumer(SubscriptionOptions options)
    {
        // At-least-once and replay require JetStream (persistence, acknowledgement, positioned start); plain
        // at-most-once uses a Core subscription (fire-and-forget, no redelivery).
        var useJetStream = options.Delivery == DeliveryMode.AtLeastOnce || options is ReplaySubscriptionOptions;
        return useJetStream
            ? new NatsJetStreamConsumer(_connection, options, Logger)
            : new NatsCoreConsumer(_connection, options, Logger);
    }
}
