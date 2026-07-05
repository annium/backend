using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for subscription construction validation and idempotent disposal via the public API.
/// </summary>
public class PipelineLifecycleTests : MessageBusTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineLifecycleTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public PipelineLifecycleTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Invalid Prefetch/Concurrency combinations are rejected when subscribing (§8.3).
    /// </summary>
    /// <param name="prefetch">The prefetch value.</param>
    /// <param name="concurrency">The concurrency value.</param>
    /// <returns>A task representing the test.</returns>
    [Theory]
    [InlineData(0, 1)] // prefetch < 1
    [InlineData(1, 0)] // concurrency < 1
    [InlineData(1, 2)] // concurrency > prefetch
    public async Task InvalidOptions_Throw(int prefetch, int concurrency)
    {
        var options = new SubscriptionOptions
        {
            Subject = "orders.created",
            Prefetch = prefetch,
            Concurrency = concurrency,
        };

        await Wrap.It(async () => await Subscriber.SubscribeAsync<Order>(options, (_, _) => Task.CompletedTask))
            .ThrowsAsync<ArgumentException>();
    }

    /// <summary>
    /// Disposing a subscription twice is safe (idempotent).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task DoubleDispose_IsSafe()
    {
        var subscription = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await subscription.DisposeAsync();
        await subscription.DisposeAsync();
    }
}
