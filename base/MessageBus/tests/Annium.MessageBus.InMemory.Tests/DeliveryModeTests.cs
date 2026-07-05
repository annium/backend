using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Delivery-mode redelivery semantics on the transport abandon path (AC3).
/// </summary>
public class DeliveryModeTests : MessageBusTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryModeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DeliveryModeTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// AC3: under AtLeastOnce, a message whose handler faults (abandon) is redelivered until handled.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task AtLeastOnce_RedeliversOnFailure()
    {
        var attempts = 0;
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created", Delivery = DeliveryMode.AtLeastOnce },
            (ctx, _) =>
            {
                var n = Interlocked.Increment(ref attempts);
                if (n == 1)
                    throw new InvalidOperationException("transient failure"); // no disposition → abandon → redeliver
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(1));

        await Expect.ToAsync(() => (Volatile.Read(ref attempts) >= 2).Is(true), 3000);
    }

    /// <summary>
    /// AC3: under AtMostOnce, a message whose handler faults is NOT redelivered.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task AtMostOnce_DropsOnFailure()
    {
        var attempts = 0;
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created", Delivery = DeliveryMode.AtMostOnce },
            (_, _) =>
            {
                Interlocked.Increment(ref attempts);
                throw new InvalidOperationException("always fails");
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(1));

        // allow any (erroneous) redelivery to occur, then assert the handler ran exactly once
        await Task.Delay(300, TestContext.Current.CancellationToken);
        Volatile.Read(ref attempts).Is(1);
    }
}
