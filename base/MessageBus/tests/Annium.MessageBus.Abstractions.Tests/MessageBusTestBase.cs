using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// Base class for pipeline tests: wires JSON serialization, a <see cref="FakeTransport"/>, and the message-bus core
/// through DI, and exposes the resolved public publisher/subscriber plus the transport for assertions.
/// </summary>
public abstract class MessageBusTestBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBusTestBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected MessageBusTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            container.Add<FakeTransport>().AsSelf().AsInterfaces().Singleton();
            container.AddMessageBusCore();
        });
    }

    /// <summary>
    /// Gets the resolved in-memory transport.
    /// </summary>
    protected FakeTransport Transport => Get<FakeTransport>();

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
    /// The subscriptions created via <see cref="SubscribeAsync{T}"/>, disposed on teardown.
    /// </summary>
    private readonly List<IAsyncDisposable> _subscriptions = new();

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

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        // dispose subscriptions (idempotent) before the container disposes the transport
        for (var i = _subscriptions.Count - 1; i >= 0; i--)
            await _subscriptions[i].DisposeAsync();

        await base.DisposeAsync();
    }
}

/// <summary>
/// A simple message payload used across pipeline tests.
/// </summary>
/// <param name="Id">The order identifier.</param>
public sealed record Order(int Id);
