using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for the in-process retry loop and dead-letter fallback via the public API (AC4, AC5).
/// </summary>
public class RetryToDlqTests : MessageBusTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryToDlqTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RetryToDlqTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A fast, deterministic retry policy for tests.
    /// </summary>
    /// <param name="maxAttempts">The maximum number of attempts.</param>
    /// <returns>The retry policy.</returns>
    private static RetryPolicy FastRetry(int maxAttempts) =>
        new()
        {
            MaxAttempts = maxAttempts,
            BaseDelay = TimeSpan.FromMilliseconds(1),
            Jitter = false,
        };

    /// <summary>
    /// AC4: repeated Nack(requeue:true) retries up to MaxAttempts, then dead-letters with diagnostic headers and
    /// acks (completes) the original message.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task NackRequeue_RetriesThenDeadLetters()
    {
        var invocations = 0;
        await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created", Retry = FastRetry(3) },
            (ctx, _) =>
            {
                Interlocked.Increment(ref invocations);
                ctx.Nack(requeue: true);
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(7));

        invocations.Is(3); // MaxAttempts
        Transport.Completed.Is(1);
        Transport.Abandoned.Is(0);

        var dlq = Transport.Dlq("orders.created");
        dlq.Count.Is(1);
        var message = dlq[0];
        Serializer.Deserialize<Order>(message.Body).Is(new Order(7));
        message.Headers[EnvelopeHeaders.OriginalSubject].Is("orders.created");
        message.Headers[EnvelopeHeaders.Attempts].Is("3");
        message.Headers[EnvelopeHeaders.DeathReason].Is("Nacked after 3 attempt(s).");
        message.Headers.ContainsKey(EnvelopeHeaders.FirstFailedAt).Is(true);
    }

    /// <summary>
    /// AC5: Nack(requeue:false) dead-letters immediately with no retries.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task NackNoRequeue_DeadLettersImmediately()
    {
        var invocations = 0;
        await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created", Retry = FastRetry(5) },
            (ctx, _) =>
            {
                Interlocked.Increment(ref invocations);
                ctx.Nack(requeue: false);
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(7));

        invocations.Is(1); // no retries despite MaxAttempts=5
        Transport.Completed.Is(1);

        var dlq = Transport.Dlq("orders.created");
        dlq.Count.Is(1);
        dlq[0].Headers[EnvelopeHeaders.Attempts].Is("1");
    }
}
