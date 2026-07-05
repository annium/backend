using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Tests.Shared;

/// <summary>
/// Conformance: basic publish/subscribe, in-subject ordering, batch, and read-loop resilience.
/// </summary>
/// <typeparam name="TTransport">The transport seam under test.</typeparam>
public abstract class PubSubConformanceTests<TTransport> : MessageBusConformanceTestBase<TTransport>
    where TTransport : class, IMessageBusTestTransport, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PubSubConformanceTests{TTransport}"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected PubSubConformanceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A message published to a subject is delivered to a subscriber and deserialized.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task PublishSubscribe_DeliversAndDeserializes()
    {
        var received = new List<Order>();
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                lock (received)
                    received.Add(ctx.Body);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(42));

        await Expect.ToAsync(() => received.Has(1), Timeout);
        received.At(0).Is(new Order(42));
    }

    /// <summary>
    /// With the default Concurrency=1, messages on a subject are processed in publish order.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task Concurrency1_PreservesSubjectOrder()
    {
        const int count = 20;
        var received = new List<int>();
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                lock (received)
                    received.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        for (var i = 0; i < count; i++)
            await Publisher.PublishAsync("orders.created", new Order(i));

        await Expect.ToAsync(() => received.Has(count), Timeout);
        received.SequenceEqual(Enumerable.Range(0, count)).Is(true);
    }

    /// <summary>
    /// A batch publish delivers every message in the batch.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task PublishBatch_DeliversAll()
    {
        var received = new List<int>();
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                lock (received)
                    received.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishBatchAsync("orders.created", new[] { new Order(1), new Order(2), new Order(3) });

        await Expect.ToAsync(() => received.Has(3), Timeout);
        received.OrderBy(x => x).SequenceEqual([1, 2, 3]).Is(true);
    }

    /// <summary>
    /// The read loop survives a pipeline contract violation (handler returns without ack/nack) and keeps processing
    /// subsequent messages. Uses AtMostOnce so the offending message is dropped rather than redelivered.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task HandlerContractViolation_LoopSurvives()
    {
        var received = new List<int>();
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created", Delivery = DeliveryMode.AtMostOnce },
            (ctx, _) =>
            {
                if (ctx.Body.Id == 0)
                    return Task.CompletedTask; // no ack/nack → pipeline throws → loop must survive

                lock (received)
                    received.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(0));
        await Publisher.PublishAsync("orders.created", new Order(1));
        await Publisher.PublishAsync("orders.created", new Order(2));

        await Expect.ToAsync(() => received.Has(2), Timeout);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }
}
