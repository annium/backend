using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// Keyed-DI smoke against a real broker: two RabbitMQ buses registered under distinct keys with distinct exchanges
/// coexist in one container and are isolated — a message published on one key's exchange is not delivered to a
/// subscriber on the other key. Demonstrates multiple exchanges via multiple keyed registrations.
/// </summary>
public sealed class RabbitKeyedExchangeTests : TestBase
{
    /// <summary>
    /// The transport seam that owns the shared broker container lifecycle.
    /// </summary>
    private readonly TestTransport _transport = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitKeyedExchangeTests"/> class, registering two keyed buses on
    /// distinct exchanges.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RabbitKeyedExchangeTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            var connectionString = TestTransport.Container.GetConnectionString();
            container.AddRabbitMqMessageBus("a", b => b.ConnectionUri(connectionString).Exchange("mb_a"));
            container.AddRabbitMqMessageBus("b", b => b.ConnectionUri(connectionString).Exchange("mb_b"));
        });
    }

    /// <summary>
    /// Brings up the shared RabbitMQ container before the DI container is built, then completes base initialization.
    /// </summary>
    /// <returns>A task that completes when initialization has finished.</returns>
    public override async ValueTask InitializeAsync()
    {
        // bring the broker up before the container is built, so the keyed registrations can read its connection string
        await _transport.StartAsync();
        await base.InitializeAsync();
    }

    /// <summary>
    /// A subscriber on key "a" (exchange mb_a) receives only messages published on "a", not those published on "b".
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task DistinctExchanges_IsolateKeyedBuses()
    {
        var subject = $"orders{Guid.NewGuid():N}";
        var received = new List<int>();

        await using var subscription = await GetKeyed<IMessageSubscriber>("a")
            .SubscribeAsync<Order>(
                new SubscriptionOptions { Subject = subject },
                (ctx, _) =>
                {
                    lock (received)
                        received.Add(ctx.Body.Id);
                    ctx.Ack();
                    return Task.CompletedTask;
                }
            );

        // published on "b" (exchange mb_b) — must not reach the subscriber bound on mb_a
        await GetKeyed<IMessagePublisher>("b").PublishAsync(subject, new Order(1));
        await Task.Delay(500, TestContext.Current.CancellationToken);
        received.IsEmpty();

        // published on "a" (exchange mb_a) — reaches the subscriber
        await GetKeyed<IMessagePublisher>("a").PublishAsync(subject, new Order(2));
        await Expect.ToAsync(() => received.Has(1), 15000);
        received.At(0).Is(2);
    }
}
