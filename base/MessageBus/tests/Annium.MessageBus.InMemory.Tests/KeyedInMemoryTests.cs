using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// End-to-end keyed-DI test: two in-memory brokers registered under distinct keys in one container are fully isolated —
/// a message published on one key is never delivered to a subscriber on the other, and each key resolves its own stack.
/// </summary>
public sealed class KeyedInMemoryTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedInMemoryTests"/> class, registering two keyed in-memory brokers.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public KeyedInMemoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            container.AddInMemoryMessageBus("a");
            container.AddInMemoryMessageBus("b");
        });
    }

    /// <summary>
    /// Each key resolves its own publisher/subscriber singleton.
    /// </summary>
    [Fact]
    public void EachKey_ResolvesOwnStack()
    {
        ReferenceEquals(GetKeyed<IMessagePublisher>("a"), GetKeyed<IMessagePublisher>("b")).Is(false);
        ReferenceEquals(GetKeyed<IMessageSubscriber>("a"), GetKeyed<IMessageSubscriber>("b")).Is(false);
    }

    /// <summary>
    /// A subscriber on key "a" receives messages published on "a" but not those published on "b".
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Publish_IsIsolatedBetweenKeys()
    {
        var received = new List<int>();
        await using var subscription = await GetKeyed<IMessageSubscriber>("a")
            .SubscribeAsync<Order>(
                new SubscriptionOptions { Subject = "orders.created" },
                (ctx, _) =>
                {
                    lock (received)
                        received.Add(ctx.Body.Id);
                    ctx.Ack();
                    return Task.CompletedTask;
                }
            );

        // published on "b" — must not reach the subscriber on "a"
        await GetKeyed<IMessagePublisher>("b").PublishAsync("orders.created", new Order(1));
        await Task.Delay(200, TestContext.Current.CancellationToken);
        received.IsEmpty();

        // published on "a" — reaches the subscriber on "a"
        await GetKeyed<IMessagePublisher>("a").PublishAsync("orders.created", new Order(2));
        await Expect.ToAsync(() => received.Has(1), 3000);
        received.At(0).Is(2);
    }
}
