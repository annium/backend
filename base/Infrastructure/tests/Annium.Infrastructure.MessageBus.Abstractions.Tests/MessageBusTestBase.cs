using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Abstractions.Tests;

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
}

/// <summary>
/// A simple message payload used across pipeline tests.
/// </summary>
/// <param name="Id">The order identifier.</param>
public sealed record Order(int Id);
