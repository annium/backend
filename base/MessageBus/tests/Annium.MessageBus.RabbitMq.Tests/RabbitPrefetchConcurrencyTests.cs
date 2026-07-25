using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// RabbitMQ-specific flow-control test: a subscription with <c>Prefetch=Concurrency=n</c> runs exactly <c>n</c> handlers
/// in parallel (proving <c>ConsumerDispatchConcurrency</c> gives parallelism) and never more than <c>n</c> in flight
/// (proving prefetch bounds unacknowledged deliveries).
/// </summary>
public sealed class RabbitPrefetchConcurrencyTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitPrefetchConcurrencyTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RabbitPrefetchConcurrencyTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Blocks all handlers at a barrier and observes that exactly <c>n</c> run concurrently while <c>n</c> more wait,
    /// then releases them and confirms every message is processed.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task PrefetchAndConcurrency_BoundInFlightAndParallelize()
    {
        const int n = 5;
        const int total = 12;
        var subject = $"orders{Guid.NewGuid():N}";

        var current = 0;
        var max = 0;
        var processed = 0;
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reachedPeak = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var subscription = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions
            {
                Subject = subject,
                Prefetch = n,
                Concurrency = n,
            },
            async (ctx, _) =>
            {
                var inFlight = Interlocked.Increment(ref current);
                RecordMax(ref max, inFlight);
                if (inFlight >= n)
                    reachedPeak.TrySetResult();

#pragma warning disable VSTHRD003 // intentionally awaiting the shared release barrier from within the handler
                await release.Task;
#pragma warning restore VSTHRD003

                Interlocked.Decrement(ref current);
                Interlocked.Increment(ref processed);
                ctx.Ack();
            }
        );

        for (var i = 0; i < total; i++)
            await Publisher.PublishAsync(subject, new Order(i));

        // n handlers reach the barrier; prefetch keeps the rest undelivered.
        await reachedPeak.Task.WaitAsync(TimeSpan.FromMilliseconds(Timeout), TestContext.Current.CancellationToken);
        Volatile.Read(ref current).Is(n); // exactly n unacked in flight (prefetch cap)
        Volatile.Read(ref max).Is(n); // n handlers ran in parallel (dispatch concurrency)

        release.SetResult();

        await Expect.ToAsync(() => processed.Is(total), Timeout);
    }

    /// <summary>
    /// Atomically raises <paramref name="max"/> to <paramref name="value"/> when larger.
    /// </summary>
    /// <param name="max">The running maximum.</param>
    /// <param name="value">The candidate value.</param>
    private static void RecordMax(ref int max, int value)
    {
        int current;
        do
        {
            current = Volatile.Read(ref max);
            if (value <= current)
                return;
        } while (Interlocked.CompareExchange(ref max, value, current) != current);
    }
}
