using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Tests.Shared;

/// <summary>
/// Conformance: consumer-group semantics — competing consumers vs fan-out.
/// </summary>
/// <typeparam name="TTransport">The transport seam under test.</typeparam>
public abstract class GroupFanoutConformanceTests<TTransport> : MessageBusConformanceTestBase<TTransport>
    where TTransport : class, IMessageBusTestTransport, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GroupFanoutConformanceTests{TTransport}"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected GroupFanoutConformanceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Two subscribers sharing a group compete — each message goes to exactly one of them.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task SameGroup_Competing()
    {
        const int count = 10;
        var a = new List<int>();
        var b = new List<int>();

        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created", Group = Group },
            (ctx, _) => Collect(a, ctx)
        );
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created", Group = Group },
            (ctx, _) => Collect(b, ctx)
        );

        for (var i = 0; i < count; i++)
            await Publisher.PublishAsync("orders.created", new Order(i));

        await Expect.ToAsync(() => (a.Count + b.Count).Is(count), Timeout);

        int[] union;
        lock (a)
            lock (b)
                union = a.Concat(b).OrderBy(x => x).ToArray();
        union.SequenceEqual(Enumerable.Range(0, count)).Is(true);
    }

    /// <summary>
    /// Subscribers with no group each receive every message (fan-out).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task NullGroup_FanOut()
    {
        const int count = 5;
        var a = new List<int>();
        var b = new List<int>();

        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) => Collect(a, ctx)
        );
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) => Collect(b, ctx)
        );

        for (var i = 0; i < count; i++)
            await Publisher.PublishAsync("orders.created", new Order(i));

        await Expect.ToAsync(
            () =>
            {
                a.Has(count);
                b.Has(count);
            },
            Timeout
        );
    }

    /// <summary>
    /// A group is scoped per subject — two subscribers sharing a group name but on different subjects do not share a
    /// channel and each receives only its own subject's messages.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task SameGroup_DifferentSubjects_AreIndependent()
    {
        var a = new List<int>();
        var b = new List<int>();

        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.a", Group = Group },
            (ctx, _) => Collect(a, ctx)
        );
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.b", Group = Group },
            (ctx, _) => Collect(b, ctx)
        );

        await Publisher.PublishAsync("orders.a", new Order(1));
        await Publisher.PublishAsync("orders.b", new Order(2));

        await Expect.ToAsync(
            () =>
            {
                a.Has(1);
                b.Has(1);
            },
            Timeout
        );
        a.At(0).Is(1);
        b.At(0).Is(2);
    }

    /// <summary>
    /// Within a group, disposing one competing consumer leaves the other consuming all messages.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task CompetingConsumer_DisposeOne_OtherKeepsConsuming()
    {
        const int count = 8;
        var a = new List<int>();
        var b = new List<int>();
        var options = new SubscriptionOptions { Subject = "orders.created", Group = Group };

        var subA = await SubscribeAsync<Order>(options, (ctx, _) => Collect(a, ctx));
        await SubscribeAsync<Order>(options, (ctx, _) => Collect(b, ctx));

        await subA.DisposeAsync(); // A leaves; the group stays alive for B

        for (var i = 0; i < count; i++)
            await Publisher.PublishAsync("orders.created", new Order(i));

        await Expect.ToAsync(() => b.Has(count), Timeout);
        a.IsEmpty();
    }
}
