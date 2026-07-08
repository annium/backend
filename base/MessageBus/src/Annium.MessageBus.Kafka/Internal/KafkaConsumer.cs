using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using Confluent.Kafka;

namespace Annium.MessageBus.Kafka.Internal;

/// <summary>
/// A Kafka transport consumer bound to a single subscription. Runs a background poll loop over one Confluent consumer,
/// invoking the pipeline callback per message. The Confluent consumer is single-threaded, so every consumer operation
/// (Consume / StoreOffset / Seek / Close) is serialized on <see cref="_lock"/>.
/// </summary>
/// <remarks>
/// Commit model: under at-most-once the offset is stored <em>before</em> the handler runs (commit-before-process, so a
/// fault is not redelivered); under at-least-once the offset is stored <em>after</em> <see cref="CompleteAsync"/> as the
/// largest contiguous completed prefix (<see cref="KafkaOffsetTracker"/>), and <see cref="AbandonAsync"/> seeks back so
/// the poll loop re-reads the delivery in-session. Seek-on-abandon is exact at <c>Concurrency=1</c>; at
/// <c>Concurrency&gt;1</c> a seek races other in-flight deliveries on the partition (documented limitation).
/// </remarks>
internal sealed class KafkaConsumer : ITransportConsumer, ILogSubject
{
    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <summary>
    /// The service provider used to lazily resolve <see cref="IKafkaAdmin"/> (topic ensure + partition lookup).
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// The subscription options.
    /// </summary>
    private readonly SubscriptionOptions _options;

    /// <summary>
    /// Whether the subscription is at-least-once (vs at-most-once).
    /// </summary>
    private readonly bool _atLeastOnce;

    /// <summary>
    /// Whether the subject is a literal topic (vs a wildcard translated to a Kafka regex subscription).
    /// </summary>
    private readonly bool _isLiteral;

    /// <summary>
    /// The Kafka subscription target: a literal topic name, or a <c>^…$</c> regex for wildcards.
    /// </summary>
    private readonly string _target;

    /// <summary>
    /// The start position; <see cref="StartPosition.New"/> for a plain (non-replay) subscription.
    /// </summary>
    private readonly StartPosition _startPosition;

    /// <summary>
    /// The underlying Confluent consumer.
    /// </summary>
    private readonly IConsumer<string, string> _consumer;

    /// <summary>
    /// Per-partition contiguous-offset tracker for at-least-once commits.
    /// </summary>
    private readonly KafkaOffsetTracker _tracker = new();

    /// <summary>
    /// The end offset per partition captured at subscribe time; the "New" floor for a fresh (uncommitted) group, so a
    /// consumer that acquires a partition after messages were published still reads everything published since it
    /// subscribed.
    /// </summary>
    private readonly Dictionary<TopicPartition, long> _subscribeEnds = new();

    /// <summary>
    /// Serializes all access to the single-threaded Confluent consumer.
    /// </summary>
    private readonly Lock _lock = new();

    /// <summary>
    /// Completes when the consumer receives its first partition assignment (readiness signal).
    /// </summary>
    private readonly TaskCompletionSource _assigned = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Cancellation source stopping the poll loop.
    /// </summary>
    private readonly CancellationTokenSource _loopCts = new();

    /// <summary>
    /// The pipeline callback invoked per delivery.
    /// </summary>
    private Func<TransportDelivery, CancellationToken, Task>? _onMessage;

    /// <summary>
    /// Guards against repeated disposal and blocks offset operations once stopping (the pipeline may still ack/abandon
    /// draining handlers after this consumer is disposed — those must not touch the closed Confluent consumer).
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaConsumer"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for lazy <see cref="IKafkaAdmin"/> resolution.</param>
    /// <param name="options">The subscription options.</param>
    /// <param name="groupId">The resolved Kafka consumer group id.</param>
    /// <param name="config">The adapter configuration.</param>
    /// <param name="logger">The logger.</param>
    public KafkaConsumer(
        IServiceProvider serviceProvider,
        SubscriptionOptions options,
        string groupId,
        KafkaConfiguration config,
        ILogger logger
    )
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _atLeastOnce = options.Delivery == DeliveryMode.AtLeastOnce;
        // Plain subscriptions are not ReplaySubscriptionOptions — default to "New" (a hard cast would throw here).
        _startPosition = (options as ReplaySubscriptionOptions)?.StartPosition ?? StartPosition.New;
        _isLiteral = Subject.IsValid(options.Subject);
        _target = _isLiteral ? options.Subject : BuildRegex(options.Subject);
        Logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = BootstrapServersParser.Format(config.BootstrapServers),
            GroupId = groupId,
            EnableAutoOffsetStore = false,
            EnableAutoCommit = true,
            // Incremental rebalancing: a new group member does not revoke everyone's partitions, avoiding re-pin races.
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
            // Literal subjects wait for assignment then read new messages (Latest). Wildcard subjects cannot wait
            // (topics are discovered later), so they read discovered topics from the beginning (Earliest). Replay
            // overrides both with its start position.
            AutoOffsetReset = ResolveOffsetReset(_startPosition, _isLiteral),
            TopicMetadataRefreshIntervalMs = 1000,
            AllowAutoCreateTopics = true,
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetPartitionsAssignedHandler(OnPartitionsAssigned)
            .SetErrorHandler((_, e) => this.Error<string>("kafka consumer error: {error}", e.ToString()))
            .Build();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        // Stop the loop token, then close/dispose the consumer under the lock. The loop touches the consumer only under
        // the lock (during Consume), so a straggling poll observes a disposed handle and stops; it is not awaited here.
        await _loopCts.CancelAsync();

        lock (_lock)
        {
            try
            {
                _consumer.Close();
            }
            catch (KafkaException e)
            {
                this.Error(e);
            }

            _consumer.Dispose();
        }

        // _loopCts is intentionally NOT disposed: a straggling poll may still read its token after this returns, and a
        // canceled-but-live token merely reports IsCancellationRequested, whereas a disposed source risks a throw.
    }

    /// <inheritdoc />
    public async Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct)
    {
        _onMessage = onMessage;

        // Ensure the literal topic exists so assignment (readiness) is deterministic; wildcards discover topics later.
        // The admin is resolved lazily and may be unavailable if the container is being disposed (shutdown) — then we
        // skip the pre-ensure and fall back to per-partition watermark resolution at assignment time.
        if (_isLiteral && TryResolveAdmin() is { } admin)
        {
            await admin.EnsureTopicAsync(_options.Subject);

            // Capture the end offset per partition now (subscribe time), before any publish, as the "New" floor.
            var partitions = admin.GetPartitions(_options.Subject);
            lock (_lock)
                foreach (var tp in partitions)
                    _subscribeEnds[tp] = _consumer.QueryWatermarkOffsets(tp, TimeSpan.FromSeconds(10)).High;
        }

        lock (_lock)
            _consumer.Subscribe(_target);

        // Fire-and-forget: the loop observes its own faults and stops when the consumer is disposed. It is intentionally
        // NOT awaited on dispose — for Concurrency=1 it awaits the handler inline, and the handler only unblocks once the
        // pipeline cancels (after this consumer's DisposeAsync returns), so awaiting it here would deadlock.
        _ = Task.Run(() => RunLoopAsync(_loopCts.Token), CancellationToken.None);

        // For a literal subscription, return only once the consumer is assigned, so a subsequent publish is captured.
        if (_isLiteral)
        {
            try
            {
                await _assigned.Task.WaitAsync(TimeSpan.FromSeconds(30), ct);
            }
            catch (TimeoutException)
            {
                this.Error<string>("consumer for {target} was not assigned within the readiness timeout", _target);
            }
        }
    }

    /// <summary>
    /// Resolves <see cref="IKafkaAdmin"/> from the service provider, returning null when the provider has been disposed
    /// (container shutdown) rather than throwing.
    /// </summary>
    /// <returns>The admin service, or null if unavailable.</returns>
    private IKafkaAdmin? TryResolveAdmin()
    {
        try
        {
            return _serviceProvider.Resolve<IKafkaAdmin>();
        }
        catch (ObjectDisposedException)
        {
            this.Error<string>("service provider disposed; skipping topic ensure for {target}", _target);
            return null;
        }
    }

    /// <inheritdoc />
    public Task CompleteAsync(TransportDelivery delivery)
    {
        // At-most-once already stored the offset before the handler ran.
        if (!_atLeastOnce || delivery.Token is not TopicPartitionOffset tpo)
            return Task.CompletedTask;

        lock (_lock)
        {
            if (_isDisposed)
                return Task.CompletedTask;

            var next = _tracker.Complete(tpo.TopicPartition, tpo.Offset.Value);
            if (next is { } value)
                _consumer.StoreOffset(new TopicPartitionOffset(tpo.TopicPartition, new Offset(value)));
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AbandonAsync(TransportDelivery delivery)
    {
        // At-most-once: the offset was already advanced (commit-before-process) → the message is dropped.
        if (!_atLeastOnce || delivery.Token is not TopicPartitionOffset tpo)
            return Task.CompletedTask;

        lock (_lock)
        {
            if (_isDisposed)
                return Task.CompletedTask;

            try
            {
                // Rewind so the poll loop re-reads (in-session raw redelivery under at-least-once).
                _consumer.Seek(tpo);
            }
            catch (KafkaException e)
            {
                this.Error<string, string>(
                    "seek failed on partition {partition}: {error}",
                    tpo.TopicPartition.ToString(),
                    e.Message
                );
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// The poll loop: consumes messages under the consumer lock and dispatches them to the pipeline callback. Handler
    /// faults are logged (the pipeline has already handled them) so the loop survives.
    /// </summary>
    /// <param name="ct">The loop cancellation token.</param>
    /// <returns>A task that completes when the loop stops.</returns>
    private async Task RunLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            ConsumeResult<string, string>? result;
            try
            {
                lock (_lock)
                    result = _consumer.Consume(TimeSpan.FromMilliseconds(100));
            }
            catch (ConsumeException e)
            {
                this.Error(e);
                continue;
            }
            catch (KafkaException e)
            {
                // A rebalance callback (OnPartitionsAssigned → Committed/QueryWatermarkOffsets/OffsetsForTimes) runs
                // inside Consume and can surface a plain KafkaException on a transient broker error; keep the loop alive.
                this.Error(e);
                continue;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            if (result is null || result.IsPartitionEOF)
                continue;

            var delivery = ToDelivery(result);

            lock (_lock)
            {
                if (_isDisposed)
                    break;

                _tracker.OnDelivered(result.TopicPartition, result.Offset.Value);

                // At-most-once: store the next offset before processing so a fault is not redelivered.
                if (!_atLeastOnce)
                    _consumer.StoreOffset(new TopicPartitionOffset(result.TopicPartition, result.Offset + 1));
            }

            try
            {
                await _onMessage!(delivery, ct);
            }
            catch (Exception e)
            {
                this.Error(e);
            }
        }
    }

    /// <summary>
    /// Signals readiness and pins the start offset of each assigned partition deterministically. "New" (the default,
    /// and replay <see cref="StartPosition.New"/>) resumes from the group's committed offset when one exists, otherwise
    /// pins to the current end (high watermark) so a subsequent publish is captured without racing lazy offset-reset.
    /// Replay positions seek to the beginning, a timestamp, or an explicit offset.
    /// </summary>
    /// <param name="consumer">The consumer being assigned.</param>
    /// <param name="partitions">The assigned partitions.</param>
    /// <returns>The partition offsets to start consuming from.</returns>
    private IEnumerable<TopicPartitionOffset> OnPartitionsAssigned(
        IConsumer<string, string> consumer,
        List<TopicPartition> partitions
    )
    {
        _assigned.TrySetResult();

        return _startPosition.Match(
            onNew: () => StartFromNew(consumer, partitions),
            onEarliest: () => partitions.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning)),
            onTimestamp: timestamp =>
                consumer.OffsetsForTimes(
                    partitions.Select(tp => new TopicPartitionTimestamp(tp, new Timestamp(timestamp.UtcDateTime))),
                    TimeSpan.FromSeconds(10)
                ),
            onPosition: position => partitions.Select(tp => new TopicPartitionOffset(tp, new Offset(position)))
        );
    }

    /// <summary>
    /// Computes "new messages only" start offsets. Wildcard subscriptions cannot pin to end (topics are discovered
    /// after publish) so they fall through to their configured Earliest reset. Literal subscriptions resume from the
    /// group's committed offset when one exists, otherwise pin to the current end (high watermark) so a subsequent
    /// publish is captured without racing lazy offset-reset or a rebalance.
    /// </summary>
    /// <param name="consumer">The consumer being assigned.</param>
    /// <param name="partitions">The assigned partitions.</param>
    /// <returns>The start offsets.</returns>
    private IEnumerable<TopicPartitionOffset> StartFromNew(
        IConsumer<string, string> consumer,
        List<TopicPartition> partitions
    )
    {
        if (!_isLiteral)
            return partitions.Select(tp => new TopicPartitionOffset(tp, Offset.Unset)); // wildcard → Earliest reset

        var committed = consumer.Committed(partitions, TimeSpan.FromSeconds(10));
        return partitions.Select(tp =>
        {
            var offset = committed.FirstOrDefault(c => c.TopicPartition == tp)?.Offset ?? Offset.Unset;
            if (offset != Offset.Unset && offset.Value >= 0)
                return new TopicPartitionOffset(tp, offset); // resume from commit

            if (_subscribeEnds.TryGetValue(tp, out var end))
                return new TopicPartitionOffset(tp, new Offset(end)); // new = end captured at subscribe time

            var watermarks = consumer.QueryWatermarkOffsets(tp, TimeSpan.FromSeconds(10));
            return new TopicPartitionOffset(tp, watermarks.High); // fallback: current end
        });
    }

    /// <summary>
    /// Builds a <see cref="TransportDelivery"/> from a Confluent consume result, decoding headers from UTF-8 and
    /// carrying the topic-partition-offset as the acknowledgement token.
    /// </summary>
    /// <param name="result">The consume result.</param>
    /// <returns>The transport delivery.</returns>
    private static TransportDelivery ToDelivery(ConsumeResult<string, string> result)
    {
        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        if (result.Message.Headers is not null)
            foreach (var header in result.Message.Headers)
                headers[header.Key] = Encoding.UTF8.GetString(header.GetValueBytes() ?? []);

        var message = new TransportMessage(result.Topic, result.Message.Value, headers, result.Message.Key);
        return new TransportDelivery(message, result.TopicPartitionOffset);
    }

    /// <summary>
    /// Resolves the Kafka auto-offset-reset from the start position. "New" reads only new messages — Latest for a
    /// literal subject (which waits for assignment), but Earliest for a wildcard (whose topics are discovered after
    /// publish and so must be read from the beginning). Explicit positions additionally seek in
    /// <see cref="OnPartitionsAssigned"/>.
    /// </summary>
    /// <param name="startPosition">The start position.</param>
    /// <param name="isLiteral">Whether the subject is a literal topic (vs a wildcard).</param>
    /// <returns>The auto-offset-reset to configure.</returns>
    private static AutoOffsetReset ResolveOffsetReset(StartPosition startPosition, bool isLiteral) =>
        startPosition.Match(
            onNew: () => isLiteral ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest,
            onEarliest: () => AutoOffsetReset.Earliest,
            onTimestamp: _ => AutoOffsetReset.Earliest,
            onPosition: _ => AutoOffsetReset.Earliest
        );

    /// <summary>
    /// Translates a canonical wildcard pattern into an anchored Kafka topic regex (<c>*</c> → one token, <c>&gt;</c> →
    /// trailing tokens). See feature spec §8.2.3.
    /// </summary>
    /// <param name="pattern">The canonical wildcard subject.</param>
    /// <returns>The <c>^…$</c> regex Kafka interprets as a topic subscription pattern.</returns>
    private static string BuildRegex(string pattern)
    {
        var parsed = SubjectPattern.Parse(pattern);
        var parts = parsed.Tokens.Select(token =>
            token switch
            {
                "*" => "[^.]+",
                ">" => ".+",
                _ => Regex.Escape(token),
            }
        );
        return "^" + string.Join("\\.", parts) + "$";
    }
}
