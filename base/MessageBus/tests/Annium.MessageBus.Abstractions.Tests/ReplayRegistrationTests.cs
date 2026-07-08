using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// White-box tests for the replay opt-in wiring of <c>AddMessageBusCore</c>: when an adapter opts into replay, the
/// resolved <see cref="IMessageSubscriber"/> also implements <see cref="IReplayableMessageSubscriber"/> (same singleton); otherwise
/// it does not, and <see cref="IReplayableMessageSubscriber"/> is not registered.
/// </summary>
/// <param name="outputHelper">The test output helper.</param>
/// <param name="supportsReplay">Whether the core is registered with replay support.</param>
public abstract class ReplayRegistrationTestsBase(ITestOutputHelper outputHelper, bool supportsReplay)
    : TestBase(outputHelper)
{
    /// <summary>
    /// Registers JSON serialization, a fake transport, and the message-bus core with the configured replay flag.
    /// </summary>
    protected void Configure()
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            container.Add<FakeTransport>().AsSelf().AsInterfaces().Singleton();
            container.AddMessageBusCore(options => options.SupportsReplay = supportsReplay);
        });
    }
}

/// <summary>
/// Replay-enabled registration: the subscriber is detectable as <see cref="IReplayableMessageSubscriber"/>.
/// </summary>
public sealed class ReplaySupportedRegistrationTests : ReplayRegistrationTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySupportedRegistrationTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ReplaySupportedRegistrationTests(ITestOutputHelper outputHelper)
        : base(outputHelper, supportsReplay: true) => Configure();

    /// <summary>
    /// When replay is opted in, the resolved subscriber implements <see cref="IReplayableMessageSubscriber"/> and is the same
    /// singleton resolvable as <see cref="IReplayableMessageSubscriber"/>.
    /// </summary>
    [Fact]
    public void Subscriber_IsReplayCapable_AndSameSingleton()
    {
        var subscriber = Get<IMessageSubscriber>();
        var replay = Get<IReplayableMessageSubscriber>();

        (subscriber is IReplayableMessageSubscriber).Is(true);
        ReferenceEquals(subscriber, replay).Is(true);
    }
}

/// <summary>
/// Default registration: the subscriber does not support replay.
/// </summary>
public sealed class ReplayUnsupportedRegistrationTests : ReplayRegistrationTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplayUnsupportedRegistrationTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ReplayUnsupportedRegistrationTests(ITestOutputHelper outputHelper)
        : base(outputHelper, supportsReplay: false) => Configure();

    /// <summary>
    /// Without replay opt-in, the resolved subscriber does not implement <see cref="IReplayableMessageSubscriber"/>.
    /// </summary>
    [Fact]
    public void Subscriber_IsNotReplayCapable()
    {
        var subscriber = Get<IMessageSubscriber>();

        (subscriber is IReplayableMessageSubscriber).Is(false);
    }
}
