using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using NATS.Client.JetStream;
using Xunit;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// NATS-specific stream-validation tests: at-least-once subscriptions require an externally-provisioned JetStream
/// stream. Subscribing against an unprovisioned subject fails fast with a clear error (the adapter never creates
/// streams), while a subject captured by a provisioned stream works.
/// </summary>
public sealed class NatsStreamValidationTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NatsStreamValidationTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public NatsStreamValidationTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// An at-least-once subscription against a subject with no provisioned stream throws a clear error.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Subscribe_UnprovisionedSubject_ThrowsClearError()
    {
        var subject = $"missing.{Guid.NewGuid():N}";

        var exception = await Assert.ThrowsAsync<NatsJSException>(async () =>
            await Subscriber.SubscribeAsync<Order>(
                new SubscriptionOptions { Subject = subject },
                (ctx, _) =>
                {
                    ctx.Ack();
                    return Task.CompletedTask;
                }
            )
        );

        exception.Message.Contains("No JetStream stream").Is(true);
    }

    /// <summary>
    /// An at-least-once subscription against a subject captured by a provisioned stream delivers messages.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Subscribe_ProvisionedSubject_Delivers()
    {
        var subject = $"validated.{Guid.NewGuid():N}";
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

        await Publisher.PublishAsync(subject, new Order(9));

        await Expect.ToAsync(() => received.Has(1), Timeout);
        received.At(0).Is(9);
    }
}
