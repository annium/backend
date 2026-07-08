using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Annium.MessageBus.RabbitMq.Internal;

/// <summary>
/// A RabbitMQ transport consumer bound to a single subscription. On start, it declares its queue (a shared durable-less
/// queue per group+subject for competing consumers, or a server-named exclusive queue for fan-out), binds it to the
/// topic exchange with the translated routing key, sets the prefetch window, and registers an async consumer. Delivery
/// tags are the acknowledgement tokens.
/// </summary>
/// <remarks>
/// Commit model: under at-most-once the consumer is registered with <c>autoAck</c> (the broker considers the message
/// delivered immediately, so a handler fault does not redeliver); under at-least-once <see cref="CompleteAsync"/> acks
/// the delivery tag and <see cref="AbandonAsync"/> nacks it with requeue (raw redelivery). Acks/nacks arriving after
/// disposal (a draining handler completing after the channel is closed) are swallowed rather than surfaced.
/// </remarks>
internal sealed class RabbitMqConsumer : ITransportConsumer, ILogSubject
{
    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <summary>
    /// The shared connection (channel factory + exchange).
    /// </summary>
    private readonly RabbitMqConnection _connection;

    /// <summary>
    /// The subscription options.
    /// </summary>
    private readonly SubscriptionOptions _options;

    /// <summary>
    /// Whether the subscription is at-least-once (vs at-most-once / autoAck).
    /// </summary>
    private readonly bool _atLeastOnce;

    /// <summary>
    /// Cancellation source signaling the consumer is stopping; its token is passed to the pipeline callback.
    /// </summary>
    private readonly CancellationTokenSource _stopCts = new();

    /// <summary>
    /// The consumer channel (one per subscription), created in <see cref="StartAsync"/>.
    /// </summary>
    private IChannel? _channel;

    /// <summary>
    /// The broker-assigned consumer tag, used to cancel the consumer on dispose.
    /// </summary>
    private string? _consumerTag;

    /// <summary>
    /// The pipeline callback invoked per delivery.
    /// </summary>
    private Func<TransportDelivery, CancellationToken, Task>? _onMessage;

    /// <summary>
    /// Guards against repeated disposal and blocks acks/nacks once stopping (a draining handler may still ack after the
    /// channel is closed).
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqConsumer"/> class.
    /// </summary>
    /// <param name="connection">The shared connection.</param>
    /// <param name="options">The subscription options.</param>
    /// <param name="logger">The logger.</param>
    public RabbitMqConsumer(RabbitMqConnection connection, SubscriptionOptions options, ILogger logger)
    {
        _connection = connection;
        _options = options;
        _atLeastOnce = options.Delivery == DeliveryMode.AtLeastOnce;
        Logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        await _stopCts.CancelAsync();

        if (_channel is { } channel)
        {
            try
            {
                if (_consumerTag is { } tag && channel.IsOpen)
                    await channel.BasicCancelAsync(tag);
                await channel.CloseAsync();
            }
            catch (Exception e) when (e is ObjectDisposedException or OperationInterruptedException)
            {
                // channel already gone (e.g. connection dropped / recovering) — nothing to release
            }
            catch (Exception e)
            {
                this.Error<string>("rabbitmq consumer channel close failed: {error}", e.Message);
            }

            await channel.DisposeAsync();
        }

        // _stopCts is intentionally NOT disposed: its token was handed to the pipeline callback and a straggling
        // handler may still read it after this returns; a canceled-but-live token merely reports cancellation, whereas
        // a disposed source risks an ObjectDisposedException.
    }

    /// <inheritdoc />
    public async Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct)
    {
        _onMessage = onMessage;

        // A dedicated channel per subscription; consumer-dispatch concurrency mirrors the requested handler concurrency.
        var channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: false,
                publisherConfirmationTrackingEnabled: false,
                consumerDispatchConcurrency: (ushort)Math.Max(1, _options.Concurrency)
            ),
            ct
        );
        _channel = channel;

        // Group set → a shared queue scoped by group+subject (competing consumers); Group null → a server-named
        // exclusive queue (fan-out, one per subscriber). Both auto-delete once their last consumer leaves.
        string queue;
        if (_options.Group is { } group)
        {
            queue = RoutingKeyTranslator.QueueName(group, _options.Subject);
            await channel.QueueDeclareAsync(
                queue,
                durable: false,
                exclusive: false,
                autoDelete: true,
                cancellationToken: ct
            );
        }
        else
        {
            var declared = await channel.QueueDeclareAsync(
                string.Empty,
                durable: false,
                exclusive: true,
                autoDelete: true,
                cancellationToken: ct
            );
            queue = declared.QueueName;
        }

        var bindingKey = RoutingKeyTranslator.BindingKey(_options.Subject);
        await channel.QueueBindAsync(queue, _connection.ExchangeName, bindingKey, cancellationToken: ct);

        // Prefetch bounds the number of unacknowledged in-flight deliveries (flow control).
        await channel.BasicQosAsync(0, (ushort)_options.Prefetch, global: false, ct);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, args) => OnReceivedAsync(args);

        // autoAck under at-most-once (delivered == consumed, no redelivery on fault); explicit ack under at-least-once.
        // The binding and consumer exist before StartAsync returns, so a subsequent publish is captured (no readiness
        // race).
        _consumerTag = await channel.BasicConsumeAsync(
            queue,
            autoAck: !_atLeastOnce,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer,
            ct
        );
    }

    /// <inheritdoc />
    public async Task CompleteAsync(TransportDelivery delivery)
    {
        // At-most-once was auto-acked on delivery; nothing to confirm.
        if (!_atLeastOnce || _isDisposed || delivery.Token is not ulong deliveryTag || _channel is not { IsOpen: true })
            return;

        try
        {
            await _channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (Exception e) when (e is ObjectDisposedException or OperationInterruptedException)
        {
            // The channel closed between the guard and the ack (dispose race), or the delivery tag is stale after an
            // automatic recovery re-consumed the queue (PRECONDITION_FAILED). Either way the unacked message is
            // redelivered later — swallow rather than surface it to the pipeline.
            this.Trace("ack for delivery {tag} skipped: channel closed or stale tag", deliveryTag);
        }
    }

    /// <inheritdoc />
    public async Task AbandonAsync(TransportDelivery delivery)
    {
        // At-most-once already advanced (autoAck) → the message is dropped.
        if (!_atLeastOnce || _isDisposed || delivery.Token is not ulong deliveryTag || _channel is not { IsOpen: true })
            return;

        try
        {
            // Requeue for raw redelivery under at-least-once.
            await _channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
        }
        catch (Exception e) when (e is ObjectDisposedException or OperationInterruptedException)
        {
            // Channel closed (dispose race) or stale delivery tag after an automatic recovery — swallow; the message is
            // redelivered later anyway.
            this.Trace("nack for delivery {tag} skipped: channel closed or stale tag", deliveryTag);
        }
    }

    /// <summary>
    /// Async consumer callback: builds a transport delivery from the AMQP args and dispatches it to the pipeline. Any
    /// fault escaping the pipeline is logged so the consumer survives (the pipeline has already handled handler faults).
    /// </summary>
    /// <param name="args">The delivery event args.</param>
    /// <returns>A task that completes when the delivery has been dispatched.</returns>
    private async Task OnReceivedAsync(BasicDeliverEventArgs args)
    {
        var delivery = ToDelivery(args);
        try
        {
            await _onMessage!(delivery, _stopCts.Token);
        }
        catch (Exception e)
        {
            this.Error(e);
        }
    }

    /// <summary>
    /// Builds a <see cref="TransportDelivery"/> from AMQP delivery args, decoding the body and headers from UTF-8 and
    /// carrying the delivery tag as the acknowledgement token.
    /// </summary>
    /// <param name="args">The delivery event args.</param>
    /// <returns>The transport delivery.</returns>
    private static TransportDelivery ToDelivery(BasicDeliverEventArgs args)
    {
        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        if (args.BasicProperties.Headers is { } amqpHeaders)
            foreach (var (key, value) in amqpHeaders)
                headers[key] = DecodeHeader(value);

        var body = Encoding.UTF8.GetString(args.Body.Span);
        var message = new TransportMessage(args.RoutingKey, body, headers, Key: null);
        return new TransportDelivery(message, args.DeliveryTag);
    }

    /// <summary>
    /// Decodes an AMQP header value, which RabbitMQ delivers as a UTF-8 byte array for string headers.
    /// </summary>
    /// <param name="value">The raw AMQP header value.</param>
    /// <returns>The decoded string.</returns>
    private static string DecodeHeader(object? value) =>
        value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            null => string.Empty,
            _ => value.ToString() ?? string.Empty,
        };
}
