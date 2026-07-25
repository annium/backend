using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Tests.Shared;

/// <summary>
/// Base for the transport conformance suite: wires JSON serialization and the transport under test (via
/// <typeparamref name="TTransport"/>) through DI, brings the broker up before the container is built, and exposes the
/// resolved public publisher/subscriber. Concrete adapter test classes are empty closed subclasses of the topic
/// bases; they only need a constructor.
/// </summary>
/// <typeparam name="TTransport">The transport seam for the adapter under test.</typeparam>
public abstract class MessageBusConformanceTestBase<TTransport> : TestBase
    where TTransport : class, IMessageBusTestTransport, new()
{
    /// <summary>
    /// The transport seam instance.
    /// </summary>
    private readonly TTransport _transport = new();

    /// <summary>
    /// The subscriptions created via <see cref="SubscribeAsync{T}"/>, disposed on teardown.
    /// </summary>
    private readonly List<IAsyncDisposable> _subscriptions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBusConformanceTestBase{TTransport}"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected MessageBusConformanceTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            _transport.Configure(container);
        });
    }

    /// <summary>
    /// Gets the resolved publisher.
    /// </summary>
    protected IMessagePublisher Publisher => Get<IMessagePublisher>();

    /// <summary>
    /// Gets the resolved subscriber.
    /// </summary>
    protected IMessageSubscriber Subscriber => Get<IMessageSubscriber>();

    /// <summary>
    /// Gets the resolved string serializer.
    /// </summary>
    protected ISerializer<string> Serializer => Get<ISerializer<string>>();

    /// <summary>
    /// Gets the eventual-assertion timeout (ms) for the transport under test.
    /// </summary>
    protected int Timeout => _transport.DefaultTimeoutMs;

    /// <summary>
    /// Gets a consumer group name unique to this test. A group maps to broker-side state that outlives the
    /// subscriptions using it (a JetStream durable, a Kafka consumer group, a RabbitMQ queue), so a fixed name would
    /// carry position and backlog from one test into the next — every message the rest of the suite publishes to the
    /// shared subject while no member is attached piles up for whoever attaches next.
    /// </summary>
    private protected string Group { get; } = $"workers-{Guid.NewGuid():N}";

    /// <summary>
    /// Brings up the transport's broker before the DI container is built, then completes base initialization.
    /// </summary>
    /// <returns>A task that completes when initialization has finished.</returns>
    public override async ValueTask InitializeAsync()
    {
        // bring the broker up before the DI container is built, so Configure can read its connection
        await _transport.StartAsync();
        await base.InitializeAsync();
    }

    /// <summary>
    /// Disposes tracked subscriptions in reverse order, then disposes the transport and completes base teardown.
    /// </summary>
    /// <returns>A task that completes when teardown has finished.</returns>
    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        // dispose subscriptions (idempotent) before tearing down the transport and the container
        for (var i = _subscriptions.Count - 1; i >= 0; i--)
            await _subscriptions[i].DisposeAsync();

        await _transport.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Subscribes via the resolved subscriber and tracks the subscription for disposal on teardown.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="options">The subscription options.</param>
    /// <param name="handler">The message handler.</param>
    /// <returns>The subscription handle (also disposed automatically on teardown).</returns>
    private protected async Task<IAsyncDisposable> SubscribeAsync<T>(
        SubscriptionOptions options,
        Func<IMessageContext<T>, CancellationToken, Task> handler
    )
        where T : notnull
    {
        var subscription = await Subscriber.SubscribeAsync(options, handler);
        _subscriptions.Add(subscription);
        return subscription;
    }

    /// <summary>
    /// Records a message id into the given sink and acks.
    /// </summary>
    /// <param name="sink">The destination list.</param>
    /// <param name="ctx">The message context.</param>
    /// <returns>A completed task.</returns>
    private protected static Task Collect(List<int> sink, IMessageContext<Order> ctx)
    {
        lock (sink)
            sink.Add(ctx.Body.Id);
        ctx.Ack();
        return Task.CompletedTask;
    }
}

/// <summary>
/// A simple message payload used across the conformance suite.
/// </summary>
/// <param name="Id">The order identifier.</param>
public sealed record Order(int Id);
