using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Tests.Shared;

/// <summary>
/// Conformance: wildcard subscription matching. Concrete subjects are distinguished by payload id.
/// </summary>
/// <typeparam name="TTransport">The transport seam under test.</typeparam>
public abstract class WildcardConformanceTests<TTransport> : MessageBusConformanceTestBase<TTransport>
    where TTransport : class, IMessageBusTestTransport, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WildcardConformanceTests{TTransport}"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected WildcardConformanceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A single-token wildcard <c>orders.*.created</c> matches three-token subjects with that shape and rejects others.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task SingleTokenWildcard_MatchesShape()
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

        await Expect.ToAsync(() => received.Has(2), Timeout);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }

    /// <summary>
    /// A multi-token wildcard <c>orders.&gt;</c> matches any subject under <c>orders.</c> and rejects others.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task MultiTokenWildcard_MatchesTail()
    {
        var received = new List<int>();
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.>" },
            (ctx, _) => Collect(received, ctx)
        );

        await Publisher.PublishAsync("orders.created", new Order(1)); // match
        await Publisher.PublishAsync("orders.eu.created", new Order(2)); // match
        await Publisher.PublishAsync("payments.created", new Order(3)); // no match

        await Expect.ToAsync(() => received.Has(2), Timeout);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }
}
