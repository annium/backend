using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Kafka wildcard conformance. Kafka matches wildcards by subscribing to a topic regex and only discovers matching
/// topics after they are created (metadata refresh), reading them from the beginning. The shared conformance bodies
/// reuse fixed subjects, which — on the run-shared broker — would accumulate history across tests; these overrides use
/// a per-test unique subject namespace so each run is isolated while exercising the same matching semantics.
/// </summary>
public sealed class WildcardTests : WildcardConformanceTests<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WildcardTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public WildcardTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A single-token wildcard (<c>*</c>) matches three-token subjects with that shape and rejects others, using a
    /// per-test unique subject namespace so the shared run-scoped broker does not accumulate history across tests.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    public override async Task SingleTokenWildcard_MatchesShape()
    {
        var ns = UniqueNamespace();
        var received = new List<int>();
        await using var subscription = await SubscribeCollectingAsync($"{ns}.*.created", received);

        await Publisher.PublishAsync($"{ns}.eu.created", new Order(1)); // match
        await Publisher.PublishAsync($"{ns}.us.created", new Order(2)); // match
        await Publisher.PublishAsync($"{ns}.created", new Order(3)); // no match (2 tokens)
        await Publisher.PublishAsync($"{ns}.eu.created.v2", new Order(4)); // no match (4 tokens)

        await Expect.ToAsync(() => received.Has(2), Timeout);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }

    /// <summary>
    /// A multi-token wildcard (<c>&gt;</c>) matches any subject under the namespace and rejects others, using a
    /// per-test unique subject namespace so the shared run-scoped broker does not accumulate history across tests.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    public override async Task MultiTokenWildcard_MatchesTail()
    {
        var ns = UniqueNamespace();
        var received = new List<int>();
        await using var subscription = await SubscribeCollectingAsync($"{ns}.>", received);

        await Publisher.PublishAsync($"{ns}.created", new Order(1)); // match
        await Publisher.PublishAsync($"{ns}.eu.created", new Order(2)); // match
        await Publisher.PublishAsync($"other{ns}.created", new Order(3)); // no match

        await Expect.ToAsync(() => received.Has(2), Timeout);
        received.OrderBy(x => x).SequenceEqual([1, 2]).Is(true);
    }

    /// <summary>
    /// Subscribes to a wildcard subject, collecting received ids into the sink.
    /// </summary>
    /// <param name="subject">The wildcard subject.</param>
    /// <param name="sink">The destination list.</param>
    /// <returns>The subscription handle.</returns>
    private Task<IAsyncDisposable> SubscribeCollectingAsync(string subject, List<int> sink) =>
        Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = subject },
            (ctx, _) =>
            {
                lock (sink)
                    sink.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

    /// <summary>
    /// Creates a unique, subject-safe namespace token for test isolation on the shared broker.
    /// </summary>
    /// <returns>The namespace token.</returns>
    private static string UniqueNamespace() => $"orders{Guid.NewGuid():N}";
}
