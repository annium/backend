using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// RabbitMQ-specific key test: a publish key is accepted and the message is delivered, but ordering by key is not
/// guaranteed (RabbitMQ has no partition/consistent-hash routing — best-effort, documented). This asserts only delivery.
/// </summary>
public sealed class RabbitKeyBestEffortTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitKeyBestEffortTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RabbitKeyBestEffortTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A message published with a key is still delivered (the key does not affect RabbitMQ routing).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task PublishWithKey_IsDelivered()
    {
        var subject = $"orders{Guid.NewGuid():N}";
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

        await Publisher.PublishAsync(subject, new Order(7), new PublishOptions { Key = "k" });

        await Expect.ToAsync(() => received.Has(1), Timeout);
        received.At(0).Is(7);
    }
}
