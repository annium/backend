using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for the headers-based envelope built on publish and round-tripped on consume, via the public API (AC9).
/// </summary>
public class EnvelopeTests : MessageBusTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnvelopeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public EnvelopeTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// AC9: publishing populates the canonical envelope headers, propagates the key and user headers, and produces
    /// a deserializable body.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Publish_PopulatesCanonicalEnvelope()
    {
        await Publisher.PublishAsync(
            "orders.created",
            new Order(42),
            new PublishOptions
            {
                Key = "k1",
                Headers = new Dictionary<string, string> { ["x-user"] = "v" },
            }
        );

        var message = Transport.Produced.Single();
        message.Subject.Is("orders.created");
        message.Key.Is("k1");
        message.Headers["x-user"].Is("v");
        Guid.TryParse(message.Headers[EnvelopeHeaders.Id], out _).Is(true);
        message.Headers[EnvelopeHeaders.Type].Is(typeof(Order).FullName!);
        message.Headers[EnvelopeHeaders.Version].Is("1");
        message.Headers[EnvelopeHeaders.ContentType].Is("application/json");
        DateTimeOffset.TryParse(message.Headers[EnvelopeHeaders.Timestamp], out _).Is(true);
        Serializer.Deserialize<Order>(message.Body).Is(new Order(42));
    }

    /// <summary>
    /// AC9: publishing a batch produces one enveloped message per item, each with a distinct auto-generated id.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task PublishBatch_ProducesEnvelopePerItem()
    {
        var orders = new[] { new Order(1), new Order(2), new Order(3) };

        await Publisher.PublishBatchAsync("orders.created", orders);

        Transport.Produced.Count.Is(3);
        var ids = new HashSet<string>();
        for (var i = 0; i < orders.Length; i++)
        {
            var message = Transport.Produced[i];
            message.Subject.Is("orders.created");
            message.Headers[EnvelopeHeaders.Type].Is(typeof(Order).FullName!);
            Serializer.Deserialize<Order>(message.Body).Is(orders[i]);
            ids.Add(message.Headers[EnvelopeHeaders.Id]);
        }
        ids.Count.Is(3); // distinct ids
    }

    /// <summary>
    /// AC9: a user-supplied id header is preserved instead of auto-generating a new one.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Publish_HonorsSuppliedId()
    {
        await Publisher.PublishAsync(
            "orders.created",
            new Order(1),
            new PublishOptions { Headers = new Dictionary<string, string> { [EnvelopeHeaders.Id] = "fixed-id" } }
        );

        Transport.Produced.Single().Headers[EnvelopeHeaders.Id].Is("fixed-id");
    }

    /// <summary>
    /// AC9: with an active <see cref="Activity"/>, the W3C trace-parent is written to the envelope.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Publish_WithActiveActivity_PropagatesTraceContext()
    {
        using var listener = new ActivityListener();
        listener.ShouldListenTo = source => source.Name == "Annium.MessageBus";
        listener.Sample = SampleAllData;
        ActivitySource.AddActivityListener(listener);

        await Publisher.PublishAsync("orders.created", new Order(1));

        Transport.Produced.Single().Headers.ContainsKey(EnvelopeHeaders.TraceParent).Is(true);
    }

    /// <summary>
    /// AC9: consuming deserializes the payload and surfaces the envelope id through the context.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Consume_RoundTripsPayloadAndId()
    {
        Order? received = null;
        string? id = null;
        await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = "orders.created" },
            (ctx, _) =>
            {
                received = ctx.Payload;
                id = ctx.Id;
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        await Publisher.PublishAsync("orders.created", new Order(9));

        received!.Is(new Order(9));
        id!.Is(Transport.Produced.Single().Headers[EnvelopeHeaders.Id]);
        Transport.Completed.Is(1);
    }

    /// <summary>
    /// Sampling callback that records all activities.
    /// </summary>
    /// <param name="options">The activity creation options.</param>
    /// <returns>Always <see cref="ActivitySamplingResult.AllData"/>.</returns>
    private static ActivitySamplingResult SampleAllData(ref ActivityCreationOptions<ActivityContext> options) =>
        ActivitySamplingResult.AllData;
}
