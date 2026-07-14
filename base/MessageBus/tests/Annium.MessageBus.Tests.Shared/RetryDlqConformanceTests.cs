using System;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Tests.Shared;

/// <summary>
/// Conformance: retry exhaustion routes to the dead-letter subject, observed black-box via a <c>.dlq</c> subscription.
/// </summary>
/// <typeparam name="TTransport">The transport seam under test.</typeparam>
public abstract class RetryDlqConformanceTests<TTransport> : MessageBusConformanceTestBase<TTransport>
    where TTransport : class, IMessageBusTestTransport, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryDlqConformanceTests{TTransport}"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected RetryDlqConformanceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A handler that keeps nacking exhausts the retry policy and the message is delivered to
    /// <c>&lt;subject&gt;.dlq</c> with the diagnostic headers.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task RetryExhaustion_DeadLetters()
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

        await Expect.ToAsync(() => dlqPayload.IsNotDefault(), Timeout);
        dlqPayload!.Is(new Order(5));
        originalSubject!.Is("orders.created");
        attempts!.Is("2");
        hasDeathReason.Is(true);
    }
}
