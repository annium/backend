using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;

namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// The broker-agnostic load harness: drives publish/subscribe against a resolved <see cref="IMessagePublisher"/> /
/// <see cref="IMessageSubscriber"/> pair and produces a <see cref="LoadRunReport"/>. It runs two scenarios — a
/// throughput / zero-loss scenario (N publishers × M messages, dedup by sequence number, baseline throughput + latency)
/// and an ordering scenario (single keyed stream at consumer <c>Concurrency=1</c>) — and asserts nothing itself; the
/// caller inspects the report (zero-loss and ordering are the acceptance criteria).
/// </summary>
public sealed class LoadHarness
{
    /// <summary>
    /// The resolved publisher.
    /// </summary>
    private readonly IMessagePublisher _publisher;

    /// <summary>
    /// The resolved subscriber.
    /// </summary>
    private readonly IMessageSubscriber _subscriber;

    /// <summary>
    /// The broker display name for the report.
    /// </summary>
    private readonly string _brokerName;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadHarness"/> class.
    /// </summary>
    /// <param name="publisher">The resolved publisher.</param>
    /// <param name="subscriber">The resolved subscriber.</param>
    /// <param name="brokerName">The broker display name.</param>
    public LoadHarness(IMessagePublisher publisher, IMessageSubscriber subscriber, string brokerName)
    {
        _publisher = publisher;
        _subscriber = subscriber;
        _brokerName = brokerName;
    }

    /// <summary>
    /// Runs the full load suite (throughput then ordering) and returns the combined report.
    /// </summary>
    /// <param name="options">The scenario options.</param>
    /// <param name="ct">A token to cancel the run.</param>
    /// <returns>The combined run report.</returns>
    public async Task<LoadRunReport> RunAsync(LoadScenarioOptions options, CancellationToken ct = default)
    {
        var throughput = await RunThroughputAsync(options, ct);
        var ordering = await RunOrderingAsync(options, ct);
        return new LoadRunReport(_brokerName, throughput, ordering);
    }

    /// <summary>
    /// Runs the throughput / zero-loss scenario: publishes <c>Publishers × MessagesPerPublisher</c> messages under
    /// at-least-once, deduplicates deliveries by sequence number, and measures wall-clock throughput and per-message
    /// latency until every produced message has been consumed at least once (or a timeout/stall aborts the wait).
    /// </summary>
    /// <param name="options">The scenario options.</param>
    /// <param name="ct">A token to cancel the run.</param>
    /// <returns>The throughput report (loss is visible as <c>ConsumedDistinct &lt; Produced</c>).</returns>
    public async Task<ThroughputReport> RunThroughputAsync(LoadScenarioOptions options, CancellationToken ct = default)
    {
        var total = options.ThroughputTotal;
        var seen = new int[total];
        var latencyMs = new double[total];
        var distinct = 0;
        var duplicates = 0L;
        var lastProgress = Stopwatch.GetTimestamp();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var subscriptionOptions = new SubscriptionOptions
        {
            Subject = options.ThroughputSubject,
            Delivery = DeliveryMode.AtLeastOnce,
            Prefetch = options.ThroughputPrefetch,
            Concurrency = options.ThroughputConcurrency,
        };

        await using var subscription = await _subscriber.SubscribeAsync<LoadMessage>(
            subscriptionOptions,
            (ctx, _) =>
            {
                var seq = ctx.Body.Seq;
                if (seq >= 0 && seq < total && Interlocked.CompareExchange(ref seen[seq], 1, 0) == 0)
                {
                    latencyMs[seq] = Stopwatch.GetElapsedTime(ctx.Body.PublishedTimestamp).TotalMilliseconds;
                    Volatile.Write(ref lastProgress, Stopwatch.GetTimestamp());
                    if (Interlocked.Increment(ref distinct) == total)
                        done.TrySetResult();
                }
                else
                {
                    Interlocked.Increment(ref duplicates);
                }

                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        var sw = Stopwatch.StartNew();

        // Publish in the background. A publish fault is a hard infrastructure failure (the broker never confirmed) —
        // ObserveFault surfaces it promptly through the completion source (so a fault mid-run ends the wait immediately),
        // and StopProducingAsync drives the producing task to a terminal state afterwards so a fault arriving *after* the
        // wait ends (e.g. on a slow publisher whose batch faults once consumption already reached the target) is never
        // swallowed — it must fail the run (exit 2), not degrade into an apparent message loss (exit 1).
        using var produceCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var producing = ProduceThroughputAsync(options, produceCts.Token);
        ObserveFault(producing, done);

        using var stallCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var stall = WatchStallAsync(
            () => Volatile.Read(ref lastProgress),
            () => Volatile.Read(ref distinct) < total,
            options.StallTimeout,
            stallCts.Token
        );

        var finished = await Task.WhenAny(done.Task, Task.Delay(options.ThroughputTimeout, ct), stall);
        sw.Stop();
        await stallCts.CancelAsync();

        if (done.Task.IsFaulted)
            await done.Task; // prompt publish fault → rethrow
        await StopProducingAsync(producing, produceCts);

        var consumed = Volatile.Read(ref distinct);
        var samples = CollectSamples(seen, latencyMs);
        var mps = sw.Elapsed.TotalSeconds > 0 ? consumed / sw.Elapsed.TotalSeconds : 0;
        // classify how the wait ended: fully drained (Completed) vs cut short by a consumption stall vs the overall
        // timeout — so a slow/quiet broker under load is reported as inconclusive, not as a delivery loss.
        var reason =
            done.Task.IsCompletedSuccessfully ? LoadStopReason.Completed
            : ReferenceEquals(finished, stall) ? LoadStopReason.Stalled
            : LoadStopReason.TimedOut;
        return new ThroughputReport(
            options.ThroughputSubject,
            total,
            consumed,
            Interlocked.Read(ref duplicates),
            sw.Elapsed,
            mps,
            Percentiles.Compute(samples),
            reason
        );
    }

    /// <summary>
    /// Runs the ordering scenario: a single publisher sends <c>OrderingMessages</c> messages one at a time under a fixed
    /// key to a single subject, consumed at <c>Concurrency=1</c>; counts first-delivery inversions (duplicates are
    /// excluded, being benign at-least-once redeliveries rather than genuine reordering).
    /// </summary>
    /// <param name="options">The scenario options.</param>
    /// <param name="ct">A token to cancel the run.</param>
    /// <returns>The ordering report.</returns>
    public async Task<OrderingReport> RunOrderingAsync(LoadScenarioOptions options, CancellationToken ct = default)
    {
        var count = options.OrderingMessages;
        var seen = new bool[count];
        var lastSeq = -1L;
        var inversions = 0L;
        var duplicates = 0L;
        var distinct = 0;
        var lastProgress = Stopwatch.GetTimestamp();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var subscriptionOptions = new SubscriptionOptions
        {
            Subject = options.OrderingSubject,
            Delivery = DeliveryMode.AtLeastOnce,
            Prefetch = 1,
            Concurrency = 1,
        };

        // Concurrency=1 → handler invocations are strictly sequential, so plain fields are safe (Volatile on lastSeq is
        // cheap visibility insurance across sequential thread hops).
        await using var subscription = await _subscriber.SubscribeAsync<LoadMessage>(
            subscriptionOptions,
            (ctx, _) =>
            {
                var seq = ctx.Body.Seq;
                if (seq >= 0 && seq < count)
                {
                    if (seen[seq])
                    {
                        duplicates++;
                    }
                    else
                    {
                        seen[seq] = true;
                        if (seq <= Volatile.Read(ref lastSeq))
                            inversions++;
                        Volatile.Write(ref lastSeq, seq);
                        Volatile.Write(ref lastProgress, Stopwatch.GetTimestamp());
                        if (++distinct == count)
                            done.TrySetResult();
                    }
                }

                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        using var produceCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var producing = ProduceOrderingAsync(options, produceCts.Token);
        ObserveFault(producing, done);

        using var stallCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var stall = WatchStallAsync(
            () => Volatile.Read(ref lastProgress),
            () => Volatile.Read(ref distinct) < count,
            options.StallTimeout,
            stallCts.Token
        );

        var finished = await Task.WhenAny(done.Task, Task.Delay(options.OrderingTimeout, ct), stall);
        await stallCts.CancelAsync();

        if (done.Task.IsFaulted)
            await done.Task; // prompt publish fault → rethrow
        await StopProducingAsync(producing, produceCts);

        var reason =
            done.Task.IsCompletedSuccessfully ? LoadStopReason.Completed
            : ReferenceEquals(finished, stall) ? LoadStopReason.Stalled
            : LoadStopReason.TimedOut;
        return new OrderingReport(
            options.OrderingSubject,
            options.OrderingKey,
            count,
            distinct,
            Interlocked.Read(ref duplicates),
            Interlocked.Read(ref inversions),
            reason
        );
    }

    /// <summary>
    /// Publishes the throughput workload: N concurrent publishers, each over a disjoint sequence range, in batches,
    /// with publish-call concurrency bounded by <see cref="LoadScenarioOptions.EffectiveMaxPublisherConcurrency"/>.
    /// </summary>
    /// <param name="options">The scenario options.</param>
    /// <param name="ct">A token to cancel publishing.</param>
    /// <returns>A task that completes when all messages have been produced (and broker-confirmed).</returns>
    private async Task ProduceThroughputAsync(LoadScenarioOptions options, CancellationToken ct)
    {
        using var gate = new SemaphoreSlim(options.EffectiveMaxPublisherConcurrency);
        var tasks = new Task[options.Publishers];
        for (var p = 0; p < options.Publishers; p++)
        {
            var publisher = p;
            tasks[publisher] = Task.Run(() => PublishRangeAsync(publisher, options, gate, ct), ct);
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Publishes one publisher's disjoint sequence range in batches, holding the shared publish gate around each
    /// <c>PublishBatchAsync</c> call.
    /// </summary>
    /// <param name="publisher">The zero-based publisher index.</param>
    /// <param name="options">The scenario options.</param>
    /// <param name="gate">The shared publish-concurrency gate.</param>
    /// <param name="ct">A token to cancel publishing.</param>
    /// <returns>A task that completes when this publisher's range has been produced.</returns>
    private async Task PublishRangeAsync(
        int publisher,
        LoadScenarioOptions options,
        SemaphoreSlim gate,
        CancellationToken ct
    )
    {
        var start = (long)publisher * options.MessagesPerPublisher;
        var end = start + options.MessagesPerPublisher;
        var batch = new List<LoadMessage>(options.BatchSize);

        for (var seq = start; seq < end; seq += options.BatchSize)
        {
            batch.Clear();
            var chunkEnd = Math.Min(seq + options.BatchSize, end);
            for (var s = seq; s < chunkEnd; s++)
                batch.Add(new LoadMessage(s, Stopwatch.GetTimestamp()));

            await gate.WaitAsync(ct);
            try
            {
                await _publisher.PublishBatchAsync(options.ThroughputSubject, batch);
            }
            finally
            {
                gate.Release();
            }
        }
    }

    /// <summary>
    /// Publishes the ordering workload: single publisher, single-message sends (never batch, to avoid producer-side
    /// reordering being an implementation detail), under a fixed key, awaited one at a time.
    /// </summary>
    /// <param name="options">The scenario options.</param>
    /// <param name="ct">A token that stops issuing further messages (individual sends are not cancelable).</param>
    /// <returns>A task that completes when all ordering messages have been produced.</returns>
    private async Task ProduceOrderingAsync(LoadScenarioOptions options, CancellationToken ct)
    {
        var publishOptions = new PublishOptions { Key = options.OrderingKey };
        for (var seq = 0; seq < options.OrderingMessages; seq++)
        {
            ct.ThrowIfCancellationRequested();
            await _publisher.PublishAsync(
                options.OrderingSubject,
                new LoadMessage(seq, Stopwatch.GetTimestamp()),
                publishOptions
            );
        }
    }

    /// <summary>
    /// Drives the producing task to a terminal state after the scenario's wait has ended: stops issuing new messages
    /// (cancellation), then awaits the task within a short grace so a genuine publish fault rethrows (failing the run)
    /// while an in-flight send wedged on an unreachable broker cannot hang the report. Our own cancellation is expected
    /// and swallowed.
    /// </summary>
    /// <param name="producing">The producing task.</param>
    /// <param name="produceCts">The producing cancellation source to trip.</param>
    /// <returns>A task that completes when producing has terminated (or the grace elapses).</returns>
    private static async Task StopProducingAsync(Task producing, CancellationTokenSource produceCts)
    {
        if (!producing.IsCompleted)
            await produceCts.CancelAsync();

        try
        {
            await producing.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (OperationCanceledException)
        {
            // producing was stopped by our own cancellation — expected, not a fault
        }
        catch (TimeoutException)
        {
            // an in-flight send has not returned (e.g. the adapter is retrying an unreachable broker) — give up
            // gracefully rather than hang; the run is already being reported as failed
        }
    }

    /// <summary>
    /// Attaches a fault observer that funnels a producing-task failure into the completion source (so the scenario
    /// awaiter rethrows the publish fault) and never leaves it unobserved.
    /// </summary>
    /// <param name="producing">The producing task.</param>
    /// <param name="done">The scenario completion source.</param>
    private static void ObserveFault(Task producing, TaskCompletionSource done)
    {
        _ = producing.ContinueWith(
            t => done.TrySetException(t.Exception!.GetBaseException()),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
    }

    /// <summary>
    /// Completes when consumption has made no progress for <paramref name="stallTimeout"/> while still incomplete —
    /// distinguishing "the broker is merely slow" (let the overall timeout run) from "consumption has stopped".
    /// </summary>
    /// <param name="lastProgress">Reads the last-progress <see cref="Stopwatch.GetTimestamp"/> value.</param>
    /// <param name="incomplete">Returns whether consumption is still incomplete.</param>
    /// <param name="stallTimeout">The no-progress window.</param>
    /// <param name="ct">A token that cancels the watchdog once the scenario finishes.</param>
    /// <returns>A task that completes on stall (or is cancelled).</returns>
    private static async Task WatchStallAsync(
        Func<long> lastProgress,
        Func<bool> incomplete,
        TimeSpan stallTimeout,
        CancellationToken ct
    )
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
                if (incomplete() && Stopwatch.GetElapsedTime(lastProgress()) > stallTimeout)
                    return;
            }
        }
        catch (OperationCanceledException)
        {
            // scenario finished — expected
        }
    }

    /// <summary>
    /// Collects the latency samples of the messages that were seen (first delivery), in ascending sequence order.
    /// </summary>
    /// <param name="seen">The per-sequence seen flags (1 = seen).</param>
    /// <param name="latencyMs">The per-sequence latency in milliseconds (written only on first delivery).</param>
    /// <returns>The dense array of latency samples.</returns>
    private static double[] CollectSamples(int[] seen, double[] latencyMs)
    {
        var samples = new List<double>(seen.Length);
        for (var i = 0; i < seen.Length; i++)
            if (seen[i] == 1)
                samples.Add(latencyMs[i]);

        return samples.ToArray();
    }
}
