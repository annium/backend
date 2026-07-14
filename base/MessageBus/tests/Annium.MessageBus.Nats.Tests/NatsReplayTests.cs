using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// NATS-specific replay tests: a replay-capable subscriber (<see cref="IReplayableMessageSubscriber"/>) consumes history
/// from a chosen <see cref="StartPosition"/> via a JetStream consumer's deliver policy. Sequence-positioned replay uses a
/// dedicated single-namespace stream so the stream sequence is deterministic.
/// </summary>
public sealed class NatsReplayTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NatsReplayTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public NatsReplayTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// The resolved replay-capable subscriber.
    /// </summary>
    private IReplayableMessageSubscriber Replay => Get<IReplayableMessageSubscriber>();

    /// <summary>
    /// The subscriber is detectable as replay-capable on NATS.
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
        var subject = $"replay.{Guid.NewGuid():N}";
        for (var i = 0; i < 3; i++)
            await Publisher.PublishAsync(subject, new Order(i));

        var received = new List<int>();
        await using var subscription = await Replay.SubscribeAsync<Order>(
            new ReplaySubscriptionOptions { Subject = subject, StartPosition = StartPosition.Earliest },
            (ctx, _) => Record(received, ctx)
        );

        await Expect.ToAsync(() => received.Has(3), Timeout);
        received.OrderBy(x => x).SequenceEqual([0, 1, 2]).Is(true);
    }

    /// <summary>
    /// StartPosition.FromPosition replays from a specific stream sequence onward (dedicated single-namespace stream, so
    /// publish order equals stream sequence).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Replay_FromPosition_ReadsFromSequence()
    {
        var subject = $"replayseq.{Guid.NewGuid():N}";
        for (var i = 0; i < 3; i++)
            await Publisher.PublishAsync(subject, new Order(i));

        var received = new List<int>();
        // Stream sequences are 1-based; start at 2 → the 2nd and 3rd messages (Order(1), Order(2)).
        await using var subscription = await Replay.SubscribeAsync<Order>(
            new ReplaySubscriptionOptions { Subject = subject, StartPosition = StartPosition.FromPosition(2) },
            (ctx, _) => Record(received, ctx)
        );

        await Expect.ToAsync(() => received.Has(2), Timeout);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }

    /// <summary>
    /// StartPosition.FromTimestamp replays only messages at or after the given time.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Replay_FromTimestamp_ReadsFromTime()
    {
        var subject = $"replay.{Guid.NewGuid():N}";

        await Publisher.PublishAsync(subject, new Order(0));
        await Publisher.PublishAsync(subject, new Order(1));
        await Task.Delay(1500, TestContext.Current.CancellationToken);
        var cutoff = DateTimeOffset.UtcNow;
        await Task.Delay(200, TestContext.Current.CancellationToken);
        await Publisher.PublishAsync(subject, new Order(2));
        await Publisher.PublishAsync(subject, new Order(3));

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
