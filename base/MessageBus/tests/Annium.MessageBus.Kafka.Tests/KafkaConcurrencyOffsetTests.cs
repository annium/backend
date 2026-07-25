using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Kafka-specific offset test: with Concurrency&gt;1 on a single partition, deliveries complete out of order. Only the
/// largest contiguous completed prefix is committed; a permanently failing message dead-letters (counted as completed)
/// which closes its gap so the prefix advances to the end. Verified black-box: a second consumer of the same group
/// resumes after the committed prefix and sees no redelivery.
/// </summary>
public sealed class KafkaConcurrencyOffsetTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaConcurrencyOffsetTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public KafkaConcurrencyOffsetTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A dead-lettered message closes its offset gap, so the whole batch is committed and nothing is redelivered.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task OutOfOrderCompletion_DlqClosesGap_CommitsWholePrefix()
    {
        const int count = 5;
        const int failing = 2;
        var subject = $"orders{Guid.NewGuid():N}";
        var group = $"g{Guid.NewGuid():N}";
        var key = new PublishOptions { Key = "k" }; // single partition

        // DLQ observer.
        var dlqIds = new List<int>();
        await using var dlq = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = $"{subject}.dlq" },
            (ctx, _) =>
            {
                lock (dlqIds)
                    dlqIds.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        // Consumer: acks all but the failing message, which keeps nacking → retry exhaustion → DLQ.
        var acked = new List<int>();
        var consumer1 = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions
            {
                Subject = subject,
                Group = group,
                Delivery = DeliveryMode.AtLeastOnce,
                Prefetch = count,
                Concurrency = 3,
                Retry = new RetryPolicy
                {
                    MaxAttempts = 2,
                    BaseDelay = TimeSpan.FromMilliseconds(1),
                    Jitter = false,
                },
            },
            (ctx, _) =>
            {
                if (ctx.Body.Id == failing)
                {
                    ctx.Nack(requeue: true);
                    return Task.CompletedTask;
                }

                lock (acked)
                    acked.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        for (var i = 0; i < count; i++)
            await Publisher.PublishAsync(subject, new Order(i), key);

        await Expect.ToAsync(
            () =>
            {
                acked.Has(count - 1); // 0,1,3,4
                dlqIds.Has(1); // 2
            },
            Timeout
        );
        dlqIds.At(0).Is(failing);

        // Let the transport-level commits for the final acks flush (the handler records the ack slightly before the
        // pipeline commits its offset), so the whole contiguous prefix is committed rather than a boundary offset left
        // uncommitted (which at-least-once would otherwise redeliver).
        await Task.Delay(1500, TestContext.Current.CancellationToken);

        await consumer1.DisposeAsync(); // commits the contiguous prefix (advanced past the dead-lettered gap)

        // A fresh consumer of the same group must see no redelivery — the whole batch was committed.
        var redelivered = new List<int>();
        await using var consumer2 = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions
            {
                Subject = subject,
                Group = group,
                Delivery = DeliveryMode.AtLeastOnce,
            },
            (ctx, _) =>
            {
                lock (redelivered)
                    redelivered.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Task.Delay(2000, TestContext.Current.CancellationToken);
        lock (redelivered)
            redelivered.IsEmpty();
    }
}
