using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using Confluent.Kafka;

namespace Annium.MessageBus.Kafka.Internal;

/// <summary>
/// The Kafka transport: a shared producer plus a consumer factory implementing the transport SPI over Confluent.Kafka.
/// Registered as a singleton by <c>AddKafkaMessageBus</c>. Canonical subjects map 1:1 to Kafka topics (dots are legal
/// in topic names); the canonical envelope headers map to Kafka message headers (UTF-8). Admin operations live in
/// <see cref="IKafkaAdmin"/>, which the transport injects into each consumer it creates.
/// </summary>
internal sealed class KafkaTransport : ITransportProducer, ITransportConsumerFactory, IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// The logger for this transport, passed on to created consumers.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The adapter configuration.
    /// </summary>
    private readonly KafkaConfiguration _config;

    /// <summary>
    /// The admin (topic ensure + partition lookup) passed to each consumer this transport creates.
    /// </summary>
    private readonly IKafkaAdmin _admin;

    /// <summary>
    /// The shared producer (thread-safe). The key is nullable — a message with no partition key produces a null key.
    /// </summary>
    private readonly IProducer<string?, string> _producer;

    /// <summary>
    /// Guards against repeated disposal (the transport is registered under two service types).
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaTransport"/> class.
    /// </summary>
    /// <param name="config">The adapter configuration.</param>
    /// <param name="admin">The admin passed to each created consumer.</param>
    /// <param name="logger">The logger passed to consumers.</param>
    public KafkaTransport(KafkaConfiguration config, IKafkaAdmin admin, ILogger logger)
    {
        _config = config;
        _admin = admin;
        Logger = logger;
        var bootstrapServers = BootstrapServersParser.Format(config.BootstrapServers);
        _producer = new ProducerBuilder<string?, string>(
            new ProducerConfig { BootstrapServers = bootstrapServers, Acks = Acks.All }
        )
            .SetErrorHandler((_, e) => this.Error<string>("kafka producer error: {error}", e.ToString()))
            .Build();
    }

    /// <summary>
    /// Produces a single message to the transport, using the canonical subject as the Kafka topic.
    /// </summary>
    /// <param name="message">The message to produce.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the message has been handed to the transport.</returns>
    public async Task ProduceAsync(TransportMessage message, CancellationToken ct)
    {
        await _producer.ProduceAsync(message.Subject, ToKafkaMessage(message), ct);
    }

    /// <summary>
    /// Produces a batch of messages to the transport. All produces are fired before any are awaited, so with
    /// <c>Acks.All</c> the batch is not serialized into one broker round-trip per message.
    /// </summary>
    /// <param name="messages">The messages to produce.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when all messages have been handed to the transport.</returns>
    public async Task ProduceBatchAsync(IReadOnlyCollection<TransportMessage> messages, CancellationToken ct)
    {
        // Fire all produces, then await together: with Acks.All, awaiting each sequentially would serialize a batch
        // into N broker round-trips and defeat librdkafka's internal batching.
        var tasks = messages.Select(message => _producer.ProduceAsync(message.Subject, ToKafkaMessage(message), ct));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Builds a Kafka message from a transport message. A null <see cref="TransportMessage.Key"/> leaves the Kafka key
    /// unset (Kafka then uses round-robin/sticky partitioning).
    /// </summary>
    /// <param name="message">The transport message.</param>
    /// <returns>The Kafka message.</returns>
    private static Message<string?, string> ToKafkaMessage(TransportMessage message) =>
        new()
        {
            Key = message.Key,
            Value = message.Body,
            Headers = ToKafkaHeaders(message.Headers),
        };

    /// <summary>
    /// Creates a consumer bound to the given subscription options. A shared <see cref="SubscriptionOptions.Group"/>
    /// maps to a shared Kafka consumer group (competing consumers); an unset group generates a unique group so every
    /// subscriber receives every message (fan-out).
    /// </summary>
    /// <param name="options">The subscription options.</param>
    /// <returns>A new transport consumer.</returns>
    public ITransportConsumer CreateConsumer(SubscriptionOptions options)
    {
        // Same Group → shared Kafka consumer group (competing); Group=null → a unique group so every subscriber gets
        // every message (fan-out).
        var groupId = options.Group ?? $"__fanout-{Guid.NewGuid():N}";
        return new KafkaConsumer(_admin, options, groupId, _config, Logger);
    }

    /// <summary>
    /// Converts canonical envelope headers to Kafka message headers (UTF-8).
    /// </summary>
    /// <param name="headers">The canonical headers.</param>
    /// <returns>The Kafka headers.</returns>
    private static Headers ToKafkaHeaders(IReadOnlyDictionary<string, string> headers)
    {
        var kafkaHeaders = new Headers();
        foreach (var (key, value) in headers)
            kafkaHeaders.Add(key, Encoding.UTF8.GetBytes(value));
        return kafkaHeaders;
    }

    /// <summary>
    /// Flushes and disposes the shared producer.
    /// </summary>
    /// <returns>A completed task, since flushing and disposal are synchronous.</returns>
    public ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return ValueTask.CompletedTask;
        _isDisposed = true;

        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();

        return ValueTask.CompletedTask;
    }
}
