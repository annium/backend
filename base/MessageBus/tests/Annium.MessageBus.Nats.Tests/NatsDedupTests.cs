using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// NATS-specific deduplication test: two publishes carrying the same message id (mirrored to the native
/// <c>Nats-Msg-Id</c> header) are deduplicated by the JetStream stream within its duplicate window, so an
/// at-least-once subscriber receives the message exactly once.
/// </summary>
public sealed class NatsDedupTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NatsDedupTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public NatsDedupTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Publishing the same id twice yields a single delivery (JetStream deduplication).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task DuplicateMsgId_DeliveredOnce()
    {
        var subject = $"dedup.{Guid.NewGuid():N}";
        var received = new List<int>();

        await using var subscription = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = subject },
            (ctx, _) =>
            {
                lock (received)
                    received.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        // A fixed message id is honored by the publish pipeline and mirrored to Nats-Msg-Id → the stream deduplicates.
        var options = new PublishOptions
        {
            Headers = new Dictionary<string, string> { [EnvelopeHeaders.Id] = "fixed-id-1" },
        };
        await Publisher.PublishAsync(subject, new Order(7), options);
        await Publisher.PublishAsync(subject, new Order(7), options);

        await Expect.ToAsync(() => received.Has(1), Timeout);
        // allow any (erroneous) duplicate delivery to arrive, then assert exactly one was received
        await Task.Delay(1000, TestContext.Current.CancellationToken);
        received.Has(1);
    }
}
