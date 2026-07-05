using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Wildcard subscription matching (AC7). Concrete subjects are distinguished by payload id.
/// </summary>
public class WildcardTests : MessageBusTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WildcardTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public WildcardTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// AC7: a single-token wildcard <c>orders.*.created</c> matches three-token subjects with that shape and rejects
    /// others.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task SingleTokenWildcard_MatchesShape()
    {
        var received = new List<int>();
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.*.created" },
            (ctx, _) => Collect(received, ctx)
        );

        await Publisher.PublishAsync("orders.eu.created", new Order(1)); // match
        await Publisher.PublishAsync("orders.us.created", new Order(2)); // match
        await Publisher.PublishAsync("orders.created", new Order(3)); // no match (2 tokens)
        await Publisher.PublishAsync("orders.eu.created.v2", new Order(4)); // no match (4 tokens)

        await Expect.ToAsync(() => received.Has(2), 3000);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }

    /// <summary>
    /// AC7: a multi-token wildcard <c>orders.&gt;</c> matches any subject under <c>orders.</c> and rejects others.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task MultiTokenWildcard_MatchesTail()
    {
        var received = new List<int>();
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.>" },
            (ctx, _) => Collect(received, ctx)
        );

        await Publisher.PublishAsync("orders.created", new Order(1)); // match
        await Publisher.PublishAsync("orders.eu.created", new Order(2)); // match
        await Publisher.PublishAsync("payments.created", new Order(3)); // no match

        await Expect.ToAsync(() => received.Has(2), 3000);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }

    /// <summary>
    /// Records a message id into the sink and acks.
    /// </summary>
    /// <param name="sink">The destination list.</param>
    /// <param name="ctx">The message context.</param>
    /// <returns>A completed task.</returns>
    private static Task Collect(List<int> sink, IMessageContext<Order> ctx)
    {
        lock (sink)
            sink.Add(ctx.Body.Id);
        ctx.Ack();
        return Task.CompletedTask;
    }
}
