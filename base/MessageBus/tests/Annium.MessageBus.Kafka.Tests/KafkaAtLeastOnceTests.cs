using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Kafka-specific at-least-once commit test: a message acked by the first consumer is committed and not redelivered,
/// while a message left unacked (consumer stops before acking) keeps its offset uncommitted and is redelivered to the
/// next consumer of the same group.
/// </summary>
public sealed class KafkaAtLeastOnceTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaAtLeastOnceTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public KafkaAtLeastOnceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// The first consumer acks message 0 (committed) but stops without acking message 1; a second consumer in the same
    /// group resumes from the committed offset and redelivers message 1 only.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task UnackedOffset_IsRedelivered_ToNextConsumer()
    {
        var subject = $"orders{Guid.NewGuid():N}";
        var group = $"g{Guid.NewGuid():N}";
        var key = new PublishOptions { Key = "k" }; // single partition → ordered offsets 0,1

        await Publisher.PublishAsync(subject, new Order(0), key);
        await Publisher.PublishAsync(subject, new Order(1), key);

        // Consumer 1: ack message 0 (commit up to 1), then hold message 1 without acking, then stop.
        var reachedMessage1 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var replay = Get<IReplayableMessageSubscriber>();
        var consumer1 = await replay.SubscribeAsync<Order>(
            new ReplaySubscriptionOptions
            {
                Subject = subject,
                Group = group,
                StartPosition = StartPosition.Earliest,
                Delivery = DeliveryMode.AtLeastOnce,
                StopTimeout = TimeSpan.FromMilliseconds(200),
            },
            async (ctx, ct) =>
            {
                if (ctx.Body.Id == 0)
                {
                    ctx.Ack();
                    return;
                }

                reachedMessage1.TrySetResult();
#pragma warning disable xUnit1051 // the handler's own token cancels the block on dispose (not the test token)
                await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, ct); // never ack message 1
#pragma warning restore xUnit1051
            }
        );

        await reachedMessage1.Task.WaitAsync(
            TimeSpan.FromMilliseconds(base.Timeout),
            TestContext.Current.CancellationToken
        );
        await consumer1.DisposeAsync(); // message 1 offset never committed

        // Consumer 2: same group resumes from the committed offset and must redeliver message 1.
        var received = new List<int>();
        await using var consumer2 = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions
            {
                Subject = subject,
                Group = group,
                Delivery = DeliveryMode.AtLeastOnce,
            },
            (ctx, _) =>
            {
                lock (received)
                    received.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Expect.ToAsync(() => received.Has(1), base.Timeout);
        received.At(0).Is(1);
    }
}
