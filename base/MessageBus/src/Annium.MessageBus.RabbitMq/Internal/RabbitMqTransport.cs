using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using RabbitMQ.Client;

namespace Annium.MessageBus.RabbitMq.Internal;

/// <summary>
/// The RabbitMQ transport: a shared publisher-confirms channel plus a consumer factory implementing the transport SPI
/// over RabbitMQ.Client v7. Registered as a singleton by <c>AddRabbitMqMessageBus</c>. Canonical subjects map to topic
/// routing keys (dots already separate tokens); the canonical envelope headers map to AMQP message headers.
/// </summary>
/// <remarks>
/// Publishing is durable against broker outages: the publish channel enables publisher confirms with tracking, so
/// <see cref="ProduceAsync"/> only completes once the broker has acknowledged the message. When the broker is
/// unreachable the publishing is buffered in an in-process retry loop (relying on the connection's automatic recovery to
/// reopen the channel) and retried until confirmed — giving zero loss across a transient outage.
/// </remarks>
internal sealed class RabbitMqTransport : ITransportProducer, ITransportConsumerFactory, IAsyncDisposable, ILogSubject
{
    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <summary>
    /// The shared connection (channel factory + exchange declaration).
    /// </summary>
    private readonly RabbitMqConnection _connection;

    /// <summary>
    /// Serializes lazy publish-channel creation.
    /// </summary>
    private readonly SemaphoreSlim _publishGate = new(1, 1);

    /// <summary>
    /// The shared publish channel (confirms enabled), created lazily. Automatic recovery reopens it after an outage, so
    /// it is created once and never replaced.
    /// </summary>
    private IChannel? _publishChannel;

    /// <summary>
    /// Guards against repeated disposal (the transport is registered under two service types).
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqTransport"/> class.
    /// </summary>
    /// <param name="connection">The shared connection.</param>
    /// <param name="logger">The logger passed to consumers.</param>
    public RabbitMqTransport(RabbitMqConnection connection, ILogger logger)
    {
        _connection = connection;
        Logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (_publishChannel is not null)
        {
            try
            {
                await _publishChannel.CloseAsync();
            }
            catch (Exception e)
            {
                this.Trace<string>("rabbitmq publish channel close failed: {error}", e.Message);
            }

            await _publishChannel.DisposeAsync();
        }

        _publishGate.Dispose();
    }

    /// <inheritdoc />
    public async Task ProduceAsync(TransportMessage message, CancellationToken ct)
    {
        var properties = ToProperties(message);
        var body = Encoding.UTF8.GetBytes(message.Body);

        var attempt = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            // Fail fast rather than retry forever once disposed — otherwise a produce racing dispose would spin on the
            // closed channel, swallow the ObjectDisposedException as a "transient" fault, and hang.
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            try
            {
                var channel = await GetPublishChannelAsync(ct);
                // Publisher confirms with tracking: this completes only once the broker acknowledges the message.
                await channel.BasicPublishAsync(
                    _connection.ExchangeName,
                    message.Subject,
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: ct
                );
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ObjectDisposedException)
            {
                // The channel was disposed (transport shutdown) mid-publish — surface it instead of retrying.
                throw;
            }
            catch (Exception e)
            {
                // Broker unreachable (outage) or channel recovering: buffer and retry until confirmed, so no message is
                // lost across a transient outage. Automatic recovery reopens the shared channel underneath.
                attempt++;
                this.Warn<string, int, string>(
                    "publish to {subject} failed (attempt {attempt}); buffering for retry: {error}",
                    message.Subject,
                    attempt,
                    e.Message
                );
                await Task.Delay(GetBackoffMs(attempt), ct);
            }
        }
    }

    /// <inheritdoc />
    public async Task ProduceBatchAsync(IReadOnlyCollection<TransportMessage> messages, CancellationToken ct)
    {
        // Serialize on the single publish channel; each publish awaits its own confirm.
        foreach (var message in messages)
            await ProduceAsync(message, ct);
    }

    /// <inheritdoc />
    public ITransportConsumer CreateConsumer(SubscriptionOptions options) =>
        new RabbitMqConsumer(_connection, options, Logger);

    /// <summary>
    /// Returns the shared publishing channel, creating it (confirms enabled) on first use.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The publishing channel.</returns>
    private async Task<IChannel> GetPublishChannelAsync(CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_publishChannel is not null)
            return _publishChannel;

        await _publishGate.WaitAsync(ct);
        try
        {
            _publishChannel ??= await _connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true
                ),
                ct
            );
            return _publishChannel;
        }
        finally
        {
            _publishGate.Release();
        }
    }

    /// <summary>
    /// Maps a transport message to AMQP basic properties: the canonical <see cref="EnvelopeHeaders.Id"/> becomes the
    /// AMQP message id, and all canonical headers are carried in the AMQP header table. Messages are marked persistent.
    /// </summary>
    /// <param name="message">The transport message.</param>
    /// <returns>The AMQP properties.</returns>
    private static BasicProperties ToProperties(TransportMessage message)
    {
        var headers = new Dictionary<string, object?>(message.Headers.Count, StringComparer.Ordinal);
        foreach (var (key, value) in message.Headers)
            headers[key] = value;

        var properties = new BasicProperties { Persistent = true, Headers = headers };
        if (message.Headers.TryGetValue(EnvelopeHeaders.Id, out var id))
            properties.MessageId = id;

        return properties;
    }

    /// <summary>
    /// Computes the capped retry backoff (milliseconds) for a failed publish attempt.
    /// </summary>
    /// <param name="attempt">The 1-based attempt number that just failed.</param>
    /// <returns>The delay before the next attempt.</returns>
    private static int GetBackoffMs(int attempt) => Math.Min(1000, 50 * attempt);
}
