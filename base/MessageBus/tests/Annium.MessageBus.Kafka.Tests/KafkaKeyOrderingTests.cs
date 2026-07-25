using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Xunit;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Kafka-specific ordering test: on a multi-partition topic, messages sharing a key route to one partition and are
/// therefore delivered in publish order under Concurrency=1. The multi-partition topic ensures the guarantee comes
/// from key-based routing, not from the topic having a single partition.
/// </summary>
public sealed class KafkaKeyOrderingTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaKeyOrderingTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public KafkaKeyOrderingTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Messages with the same key preserve order on a three-partition topic.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task SameKey_PreservesOrder_OnMultiPartitionTopic()
    {
        const int count = 30;
        var subject = $"orders{Guid.NewGuid():N}";
        await CreateTopicAsync(subject, partitions: 3);

        var received = new List<int>();
        await using var subscription = await Subscriber.SubscribeAsync<Order>(
            new SubscriptionOptions { Subject = subject }, // Concurrency=1
            (ctx, _) =>
            {
                lock (received)
                    received.Add(ctx.Body.Id);
                ctx.Ack();
                return Task.CompletedTask;
            }
        );

        var key = new PublishOptions { Key = "same-key" };
        for (var i = 0; i < count; i++)
            await Publisher.PublishAsync(subject, new Order(i), key);

        await Expect.ToAsync(() => received.Has(count), Timeout);
        received.SequenceEqual(Enumerable.Range(0, count)).Is(true);
    }

    /// <summary>
    /// Creates a topic with the given partition count using an admin client bound to the resolved bootstrap servers.
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <param name="partitions">The partition count.</param>
    /// <returns>A task that completes when the topic exists.</returns>
    private async Task CreateTopicAsync(string topic, int partitions)
    {
        var config = Get<KafkaConfiguration>();
        using var admin = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = string.Join(",", config.BootstrapServers) }
        ).Build();
        await admin.CreateTopicsAsync([
            new TopicSpecification
            {
                Name = topic,
                NumPartitions = partitions,
                ReplicationFactor = 1,
            },
        ]);
    }
}
