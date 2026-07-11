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
    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task ProduceAsync(TransportMessage message, CancellationToken ct)
    {
        var jetStream = await _connection.GetJetStreamAsync(ct);
        var headers = NatsHeaderMapper.ToNatsHeaders(message.Headers);
        // JetStream publish waits for the stream's acknowledgement (zero-loss) and applies Nats-Msg-Id deduplication.
        await jetStream.PublishAsync(message.Subject, message.Body, headers: headers, cancellationToken: ct);
    }

    /// <inheritdoc />
    public async Task ProduceBatchAsync(IReadOnlyCollection<TransportMessage> messages, CancellationToken ct)
    {
        // Fire all publishes then await together; each awaits its own stream ack, so parallel avoids serializing the
        // batch into N sequential round-trips.
        var tasks = messages.Select(message => ProduceAsync(message, ct));
        await Task.WhenAll(tasks);
    }

    /// <inheritdoc />
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
