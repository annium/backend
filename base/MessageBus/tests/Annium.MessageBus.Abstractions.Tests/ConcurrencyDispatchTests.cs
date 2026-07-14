using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for bounded-concurrency dispatch via the public API (AC7).
/// </summary>
public class ConcurrencyDispatchTests : MessageBusTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyDispatchTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ConcurrencyDispatchTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// AC7: with Concurrency=1 the pipeline runs handlers strictly sequentially and preserves subject order.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Concurrency1_SerializesAndPreservesOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var lockObj = new object();
        var current = 0;
        var max = 0;
        var order = new List<int>();

        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            async (ctx, _) =>
            {
                lock (lockObj)
                {
                    current++;
                    max = Math.Max(max, current);
                }
                await Task.Delay(10, ct);
                lock (lockObj)
                {
                    current--;
                    order.Add(ctx.Body.Id);
                }
                ctx.Ack();
            }
        );

        // Concurrency == 1: each publish awaits full inline processing, so sequential publishes stay ordered.
        for (var i = 0; i < 6; i++)
            await Publisher.PublishAsync("orders.created", new Order(i));

        max.Is(1);
        order.SequenceEqual(Enumerable.Range(0, 6)).Is(true);
    }

    /// <summary>
    /// AC7: with Concurrency=N the pipeline runs up to N handlers in parallel and no more.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task ConcurrencyN_RunsInParallelBounded()
    {
        var ct = TestContext.Current.CancellationToken;
        const int concurrency = 4;
        var lockObj = new object();
        var current = 0;
        var max = 0;

        var subscription = await SubscribeAsync<Order>(
            new SubscriptionOptions
            {
                Subject = "orders.created",
                Prefetch = concurrency,
                Concurrency = concurrency,
            },
            async (ctx, _) =>
            {
                lock (lockObj)
                {
                    current++;
                    max = Math.Max(max, current);
                }
                await Task.Delay(50, ct);
                lock (lockObj)
                    current--;
                ctx.Ack();
            }
        );

        var publishes = Enumerable
            .Range(0, 8)
            .Select(i => Publisher.PublishAsync("orders.created", new Order(i)))
            .ToArray();
        await Task.WhenAll(publishes);

        // drain all in-flight handlers before asserting
        await subscription.DisposeAsync();

        (max <= concurrency).Is(true);
        (max > 1).Is(true);
    }
}
