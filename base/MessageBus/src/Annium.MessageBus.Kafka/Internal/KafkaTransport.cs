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
/// <see cref="IKafkaAdmin"/>, which consumers resolve lazily.
/// </summary>
internal sealed class KafkaTransport : ITransportProducer, ITransportConsumerFactory, IAsyncDisposable, ILogSubject
{
    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <summary>
    /// The adapter configuration.
    /// </summary>
    private readonly KafkaConfiguration _config;

    /// <summary>
    /// The service provider consumers resolve <see cref="IKafkaAdmin"/> from (lazily, guarded against disposal).
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

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
    /// <param name="serviceProvider">The service provider passed to consumers for lazy admin resolution.</param>
    /// <param name="logger">The logger passed to consumers.</param>
    public KafkaTransport(KafkaConfiguration config, IServiceProvider serviceProvider, ILogger logger)
    {
        _config = config;
        _serviceProvider = serviceProvider;
        Logger = logger;
        var bootstrapServers = BootstrapServersParser.Format(config.BootstrapServers);
        _producer = new ProducerBuilder<string?, string>(
            new ProducerConfig { BootstrapServers = bootstrapServers, Acks = Acks.All }
        )
            .SetErrorHandler((_, e) => this.Error<string>("kafka producer error: {error}", e.ToString()))
            .Build();
    }

    /// <inheritdoc />
    public async Task ProduceAsync(TransportMessage message, CancellationToken ct)
    {
        await _producer.ProduceAsync(message.Subject, ToKafkaMessage(message), ct);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public ITransportConsumer CreateConsumer(SubscriptionOptions options)
    {
        // Same Group → shared Kafka consumer group (competing); Group=null → a unique group so every subscriber gets
        // every message (fan-out).
        var groupId = options.Group ?? $"__fanout-{Guid.NewGuid():N}";
        return new KafkaConsumer(_serviceProvider, options, groupId, _config, Logger);
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

    /// <inheritdoc />
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
