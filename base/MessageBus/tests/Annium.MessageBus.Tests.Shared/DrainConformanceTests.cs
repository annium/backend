using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Tests.Shared;

/// <summary>
/// Conformance: graceful drain on subscription disposal.
/// </summary>
/// <typeparam name="TTransport">The transport seam under test.</typeparam>
public abstract class DrainConformanceTests<TTransport> : MessageBusConformanceTestBase<TTransport>
    where TTransport : class, IMessageBusTestTransport, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DrainConformanceTests{TTransport}"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected DrainConformanceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Disposing a subscription with an in-flight handler (shorter than StopTimeout) waits for it to finish.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task Dispose_WaitsForInFlightHandler()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var subscription = await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created", StopTimeout = TimeSpan.FromSeconds(5) },
            async (ctx, _) =>
            {
                started.TrySetResult();
                await Task.Delay(200, CancellationToken.None);
                ctx.Ack();
                finished.TrySetResult();
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(1));
        await started.Task; // handler is now in-flight

        await subscription.DisposeAsync(); // must drain the in-flight handler (200ms < 5s StopTimeout)

        finished.Task.IsCompletedSuccessfully.Is(true);
    }

    /// <summary>
    /// Disposing a parallel (Concurrency&gt;1) subscription while work is in flight/queued completes without throwing
    /// and does not surface an <see cref="ObjectDisposedException"/> from a straggler handler.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public virtual async Task Dispose_WithParallelInFlight_IsClean()
    {
        var subscription = await SubscribeAsync<Order>(
            new SubscriptionOptions
            {
                Subject = "orders.created",
                Prefetch = 8,
                Concurrency = 8,
                StopTimeout = TimeSpan.FromSeconds(2),
            },
            async (ctx, _) =>
            {
                await Task.Delay(50, CancellationToken.None);
                ctx.Ack();
            }
        );

        for (var i = 0; i < 50; i++)
            await Publisher.PublishAsync("orders.created", new Order(i));

        await Task.Delay(30, TestContext.Current.CancellationToken); // let a batch start in parallel
        await subscription.DisposeAsync(); // must not throw despite in-flight + queued work

        await Task.Delay(150, TestContext.Current.CancellationToken); // let any straggler settle
        Logs.Any(m => m.Exception is ObjectDisposedException).Is(false);
    }
}
