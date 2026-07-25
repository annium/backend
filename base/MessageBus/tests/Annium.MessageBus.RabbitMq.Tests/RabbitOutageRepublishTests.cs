using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// RabbitMQ-specific outage-durability test: messages published while the broker is down are buffered by the transport
/// (publisher confirms + retry) and republished after recovery, so none are lost. The broker is taken down and back up
/// via <c>rabbitmqctl stop_app</c>/<c>start_app</c>, which drops connections and triggers the client's automatic
/// connection + topology recovery.
/// </summary>
public sealed class RabbitOutageRepublishTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitOutageRepublishTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RabbitOutageRepublishTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Two messages published during a broker outage are republished after recovery and delivered alongside the
    /// pre-outage messages (zero loss).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task PublishDuringOutage_RepublishesAfterRecovery()
    {
        var subject = $"orders{Guid.NewGuid():N}";
        var group = $"g{Guid.NewGuid():N}";
        var received = new List<int>();

        await using var subscription = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = subject, Group = group },
            (ctx, _) =>
            {
                lock (received)
                    received.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        // baseline: two messages before the outage
        await Publisher.PublishAsync(subject, new Order(0));
        await Publisher.PublishAsync(subject, new Order(1));
        await Expect.ToAsync(() => received.Has(2), Timeout);

        // take the broker down: connections drop, publishing fails and must buffer
        await TestTransport.ControlAsync("stop_app");

        // publish during the outage — these block in the transport's retry loop until the broker returns
        var buffered = Task.WhenAll(
            Publisher.PublishAsync(subject, new Order(2)),
            Publisher.PublishAsync(subject, new Order(3))
        );
        await Task.Delay(500, TestContext.Current.CancellationToken);
        buffered.IsCompleted.Is(false); // still buffered (not confirmed) while the broker is down

        // bring the broker back: automatic recovery reconnects, topology is restored, buffered publishes confirm
        await TestTransport.ControlAsync("start_app");
        await buffered.WaitAsync(TimeSpan.FromMilliseconds(Timeout * 2), TestContext.Current.CancellationToken);

        await Expect.ToAsync(() => received.Has(4), Timeout * 2);
        int[] ids;
        lock (received)
            ids = received.OrderBy(x => x).ToArray();
        ids.SequenceEqual([0, 1, 2, 3]).Is(true);
    }
}
