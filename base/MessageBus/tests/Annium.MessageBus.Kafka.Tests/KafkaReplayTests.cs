using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Kafka-specific replay tests: a replay-capable subscriber (<see cref="IReplayableMessageSubscriber"/>) consumes history from a
/// chosen <see cref="StartPosition"/>. Messages are published first (to a single-partition topic for deterministic
/// offsets), then replayed.
/// </summary>
public sealed class KafkaReplayTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaReplayTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public KafkaReplayTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// The resolved replay-capable subscriber.
    /// </summary>
    private IReplayableMessageSubscriber Replay => Get<IReplayableMessageSubscriber>();

    /// <summary>
    /// The subscriber is detectable as replay-capable on Kafka.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Subscriber_IsReplayCapable()
    {
        (Subscriber is IReplayableMessageSubscriber).Is(true);
        await Task.CompletedTask;
    }

    /// <summary>
    /// StartPosition.Earliest replays the full history of a subject.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Replay_Earliest_ReadsAllHistory()
    {
        var subject = $"replay{Guid.NewGuid():N}";
        var key = new PublishOptions { Key = "k" }; // fixed key → single partition → deterministic offsets
        for (var i = 0; i < 3; i++)
            await Publisher.PublishAsync(subject, new Order(i), key);

        var received = new List<int>();
        await using var subscription = await Replay.SubscribeAsync<Order>(
            new ReplaySubscriptionOptions { Subject = subject, StartPosition = StartPosition.Earliest },
            (ctx, _) => Record(received, ctx)
        );

        await Expect.ToAsync(() => received.Has(3), Timeout);
        received.OrderBy(x => x).SequenceEqual([0, 1, 2]).Is(true);
    }

    /// <summary>
    /// StartPosition.FromPosition replays from a specific offset onward (single-partition topic → publish order equals
    /// offset order).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Replay_FromPosition_ReadsFromOffset()
    {
        var subject = $"replay{Guid.NewGuid():N}";
        var key = new PublishOptions { Key = "k" }; // fixed key → single partition → deterministic offsets
        for (var i = 0; i < 3; i++)
            await Publisher.PublishAsync(subject, new Order(i), key);

        var received = new List<int>();
        await using var subscription = await Replay.SubscribeAsync<Order>(
            new ReplaySubscriptionOptions { Subject = subject, StartPosition = StartPosition.FromPosition(1) },
            (ctx, _) => Record(received, ctx)
        );

        await Expect.ToAsync(() => received.Has(2), Timeout);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }

    /// <summary>
    /// StartPosition.FromTimestamp replays only messages at or after the given time (single-partition topic so the
    /// timestamp split is deterministic).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Replay_FromTimestamp_ReadsFromTime()
    {
        var subject = $"replay{Guid.NewGuid():N}";
        var key = new PublishOptions { Key = "k" }; // fixed key → single partition

        await Publisher.PublishAsync(subject, new Order(0), key);
        await Publisher.PublishAsync(subject, new Order(1), key);
        await Task.Delay(1500, TestContext.Current.CancellationToken);
        var cutoff = DateTimeOffset.UtcNow;
        await Task.Delay(200, TestContext.Current.CancellationToken);
        await Publisher.PublishAsync(subject, new Order(2), key);
        await Publisher.PublishAsync(subject, new Order(3), key);

        var received = new List<int>();
        await using var subscription = await Replay.SubscribeAsync<Order>(
            new ReplaySubscriptionOptions { Subject = subject, StartPosition = StartPosition.FromTimestamp(cutoff) },
            (ctx, _) => Record(received, ctx)
        );

        await Expect.ToAsync(() => received.Has(2), Timeout);
        received.OrderBy(x => x).SequenceEqual([2, 3]).Is(true);
    }

    /// <summary>
    /// Records a message id into the sink and acks.
    /// </summary>
    /// <param name="sink">The destination list.</param>
    /// <param name="ctx">The message context.</param>
    /// <returns>A completed task.</returns>
    private static Task Record(List<int> sink, IMessageContext<Order> ctx)
    {
        lock (sink)
            sink.Add(ctx.Body.Id);
        ctx.Ack();
        return Task.CompletedTask;
    }
}
