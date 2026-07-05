using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests that the pipeline emits activity spans and meter measurements, via the public API (AC8).
/// </summary>
public class ObservabilityTests : MessageBusTestBase
{
    /// <summary>
    /// The instrumentation name of the message-bus activity source and meter.
    /// </summary>
    private const string InstrumentationName = "Annium.MessageBus";

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservabilityTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ObservabilityTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// AC8: publishing and consuming (with ack, and with a nack→retry→dlq flow) emit the full set of counters, the
    /// latency histogram, and producer/consumer spans.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task PublishAndConsume_EmitCountersAndSpans()
    {
        var counters = new ConcurrentDictionary<string, long>();
        var latencyCount = 0;
        var spanNames = new ConcurrentBag<string>();

        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == InstrumentationName)
                l.EnableMeasurementEvents(instrument);
        };
        meterListener.SetMeasurementEventCallback<long>(
            (instrument, measurement, _, _) =>
                counters.AddOrUpdate(instrument.Name, measurement, (_, old) => old + measurement)
        );
        meterListener.SetMeasurementEventCallback<double>((_, _, _, _) => Interlocked.Increment(ref latencyCount));
        meterListener.Start();

        using var activityListener = new ActivityListener();
        activityListener.ShouldListenTo = source => source.Name == InstrumentationName;
        activityListener.Sample = SampleAllData;
        activityListener.ActivityStarted = activity => spanNames.Add(activity.DisplayName);
        ActivitySource.AddActivityListener(activityListener);

        // consume with ack → consume/ack counters, latency, consumer span; publish → publish counter + producer span
        await SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                ctx.Ack();
                return Task.CompletedTask;
            }
        );
        await Publisher.PublishAsync("orders.created", new Order(1));

        // consume with nack → retry → dlq on another subject → nack/retry/dlq counters
        await SubscribeAsync<Order>(
            new SubscriptionOptions
            {
                Subject = "orders.failed",
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
        await Publisher.PublishAsync("orders.failed", new Order(2));

        Present(counters, "messagebus.publish");
        Present(counters, "messagebus.consume");
        Present(counters, "messagebus.ack");
        Present(counters, "messagebus.nack");
        Present(counters, "messagebus.retry");
        Present(counters, "messagebus.dlq");
        (latencyCount > 0).Is(true);
        HasSpanSuffix(spanNames, "publish").Is(true);
        HasSpanSuffix(spanNames, "consume").Is(true);
    }

    /// <summary>
    /// Asserts a counter was recorded with a positive value.
    /// </summary>
    /// <param name="counters">The captured counters.</param>
    /// <param name="name">The instrument name.</param>
    private static void Present(ConcurrentDictionary<string, long> counters, string name) =>
        (counters.TryGetValue(name, out var value) && value > 0).Is(true);

    /// <summary>
    /// Determines whether any captured span display name ends with the given suffix.
    /// </summary>
    /// <param name="spanNames">The captured span names.</param>
    /// <param name="suffix">The suffix to look for.</param>
    /// <returns>True when a matching span was captured.</returns>
    private static bool HasSpanSuffix(IEnumerable<string> spanNames, string suffix)
    {
        foreach (var name in spanNames)
            if (name.EndsWith(suffix, StringComparison.Ordinal))
                return true;
        return false;
    }

    /// <summary>
    /// Sampling callback that records all activities.
    /// </summary>
    /// <param name="options">The activity creation options.</param>
    /// <returns>Always <see cref="ActivitySamplingResult.AllData"/>.</returns>
    private static ActivitySamplingResult SampleAllData(ref ActivityCreationOptions<ActivityContext> options) =>
        ActivitySamplingResult.AllData;
}
