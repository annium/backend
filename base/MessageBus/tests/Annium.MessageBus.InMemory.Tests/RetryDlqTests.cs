using System;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Retry exhaustion routes to the dead-letter subject (AC5), observed black-box via a <c>.dlq</c> subscription.
/// </summary>
public class RetryDlqTests : MessageBusTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryDlqTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RetryDlqTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// AC5: a handler that keeps nacking exhausts the retry policy and the message is delivered to
    /// <c>&lt;subject&gt;.dlq</c> with the diagnostic headers.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task RetryExhaustion_DeadLetters()
    {
        Order? dlqPayload = null;
        string? originalSubject = null;
        string? attempts = null;
        var hasDeathReason = false;

        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created.dlq" },
            (ctx, _) =>
            {
                dlqPayload = ctx.Body;
                originalSubject = ctx.Headers[EnvelopeHeaders.OriginalSubject];
                attempts = ctx.Headers[EnvelopeHeaders.Attempts];
                hasDeathReason = ctx.Headers.ContainsKey(EnvelopeHeaders.DeathReason);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await SubscribeAsync<Order>(
            new SubscriptionOptions
            {
                Subject = "orders.created",
                Retry = new RetryPolicy
                {
                    MaxAttempts = 2,
                    BaseDelay = TimeSpan.FromMilliseconds(1),
                    Jitter = false,
                },
            },
            (ctx, _) =>
            {
                ctx.Nack(requeue: true);
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(5));

        await Expect.ToAsync(() => dlqPayload.IsNotDefault(), 3000);
        dlqPayload!.Is(new Order(5));
        originalSubject!.Is("orders.created");
        attempts!.Is("2");
        hasDeathReason.Is(true);
    }
}
