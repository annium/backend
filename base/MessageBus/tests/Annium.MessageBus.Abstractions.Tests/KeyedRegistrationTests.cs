using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// White-box tests for keyed registration: two brokers registered under distinct keys resolve to independent stacks
/// (distinct publishers/subscribers/transports) and are isolated; a keyed-only registration is not exposed non-keyed.
/// </summary>
public sealed class KeyedRegistrationTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedRegistrationTests"/> class, registering two keyed fake brokers.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public KeyedRegistrationTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            RegisterBus(container, "a");
            RegisterBus(container, "b");
        });
    }

    /// <summary>
    /// Registers a fake transport (one instance under both SPI types) and the core, all keyed by <paramref name="key"/>.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="key">The registration key.</param>
    private static void RegisterBus(IServiceContainer container, object key)
    {
        var transport = new FakeTransport();
        container
            .Add(transport)
            .AsKeyedSelf(key)
            .AsKeyed<ITransportProducer>(key)
            .AsKeyed<ITransportConsumerFactory>(key)
            .Singleton();
        container.AddMessageBusCore(key);
    }

    /// <summary>
    /// Each key resolves its own publisher and transport singleton.
    /// </summary>
    [Fact]
    public void KeyedRegistrations_AreDistinctSingletons()
    {
        ReferenceEquals(GetKeyed<IMessagePublisher>("a"), GetKeyed<IMessagePublisher>("b")).Is(false);
        ReferenceEquals(GetKeyed<FakeTransport>("a"), GetKeyed<FakeTransport>("b")).Is(false);
        // the keyed publisher is wired to the same-key transport
        ReferenceEquals(GetKeyed<ITransportProducer>("a"), GetKeyed<FakeTransport>("a")).Is(true);
    }

    /// <summary>
    /// Publishing on one key reaches only that key's transport (isolation).
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Publish_IsIsolatedPerKey()
    {
        await GetKeyed<IMessagePublisher>("a").PublishAsync("orders.created", new Order(1));

        GetKeyed<FakeTransport>("a").Produced.Has(1);
        GetKeyed<FakeTransport>("b").Produced.IsEmpty();
    }

    /// <summary>
    /// Keyed-only registrations are not exposed non-keyed (no default forwarder).
    /// </summary>
    [Fact]
    public void KeyedOnly_HasNoNonKeyedDefault()
    {
        Provider.TryResolve<IMessagePublisher>().IsDefault();
    }
}

/// <summary>
/// White-box tests for the default (keyless) registration: the public services are exposed both under the default key
/// and non-keyed, resolving to the same singleton.
/// </summary>
public sealed class DefaultRegistrationTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRegistrationTests"/> class, registering one keyless broker.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DefaultRegistrationTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            var transport = new FakeTransport();
            container
                .Add(transport)
                .AsKeyedSelf(MessageBusKeys.Default)
                .AsKeyed<ITransportProducer>(MessageBusKeys.Default)
                .AsKeyed<ITransportConsumerFactory>(MessageBusKeys.Default)
                .Singleton();
            container.AddMessageBusCore();
        });
    }

    /// <summary>
    /// The non-keyed publisher and the default-keyed publisher are the same singleton.
    /// </summary>
    [Fact]
    public void Default_ResolvesBothKeyedAndNonKeyed_SameSingleton()
    {
        ReferenceEquals(Get<IMessagePublisher>(), GetKeyed<IMessagePublisher>(MessageBusKeys.Default)).Is(true);
        ReferenceEquals(Get<IMessageSubscriber>(), GetKeyed<IMessageSubscriber>(MessageBusKeys.Default)).Is(true);
    }
}
