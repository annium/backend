using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// RabbitMQ does not support replay: <c>AddRabbitMqMessageBus</c> registers the plain core, so the resolved subscriber
/// must not implement <see cref="IReplayableMessageSubscriber"/>.
/// </summary>
public sealed class RabbitReplayAbsenceTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitReplayAbsenceTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RabbitReplayAbsenceTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// The resolved subscriber is not replay-capable.
    /// </summary>
    [Fact]
    public void Subscriber_IsNotReplayCapable()
    {
        (Subscriber is IReplayableMessageSubscriber).Is(false);
    }
}
