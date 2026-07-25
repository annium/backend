using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for the strict ack-contract of the consumption pipeline via the public API (AC1, AC2, AC3, AC6).
/// </summary>
public class AckContractTests : MessageBusTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AckContractTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public AckContractTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// AC1: a handler that acks completes (commits) the message exactly once and never abandons it.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Ack_CommitsExactlyOnce()
    {
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(1));

        Transport.Completed.Is(1);
        Transport.Abandoned.Is(0);
    }

    /// <summary>
    /// AC2: a handler that returns without acking or nacking triggers an <see cref="InvalidOperationException"/> and
    /// leaves the message unconfirmed.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task NoDisposition_Throws_AndLeavesUnconfirmed()
    {
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (_, _) => Task.CompletedTask
        );

        await Publisher.PublishAsync("orders.created", new Order(1));

        Transport.Completed.Is(0);
        Transport.Abandoned.Is(1);
        Transport.LastConsumerError.AsExact<InvalidOperationException>();
    }

    /// <summary>
    /// AC3: a handler that throws without a disposition logs the original exception, leaves the message unconfirmed
    /// (raw redelivery), and does NOT engage the retry policy or dead-letter the message.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task HandlerThrows_LogsAndAbandons_WithoutRetry()
    {
        var invocations = 0;
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (_, _) =>
            {
                invocations++;
                throw new InvalidTimeZoneException("handler boom");
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(1));

        invocations.Is(1); // retry NOT engaged
        Transport.Completed.Is(0);
        Transport.Abandoned.Is(1);
        Transport.Dlq("orders.created").Count.Is(0);
        Logs.Any(m => m.Exception is InvalidTimeZoneException).Is(true);
    }

    /// <summary>
    /// AC6: acking twice throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task DoubleAck_Throws()
    {
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                ctx.Ack();
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(1));

        Transport.LastConsumerError.AsExact<InvalidOperationException>();
    }

    /// <summary>
    /// AC6: acking then nacking throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task AckThenNack_Throws()
    {
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                ctx.Ack();
                ctx.Nack();
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(1));

        Transport.LastConsumerError.AsExact<InvalidOperationException>();
    }
}
