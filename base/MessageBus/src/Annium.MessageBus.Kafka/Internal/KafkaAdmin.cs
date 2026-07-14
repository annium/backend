using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Annium.MessageBus.Kafka.Internal;

/// <summary>
/// The default <see cref="IKafkaAdmin"/> implementation. Owns a lazily created Confluent <c>AdminClient</c> (built once
/// on first use) and disposes it with the DI container.
/// </summary>
internal sealed class KafkaAdmin : IKafkaAdmin, IDisposable
{
    /// <summary>
    /// The lazily created admin client.
    /// </summary>
    private readonly Lazy<IAdminClient> _adminClient;

    /// <summary>
    /// Guards against repeated disposal.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaAdmin"/> class.
    /// </summary>
    /// <param name="config">The adapter configuration.</param>
    public KafkaAdmin(KafkaConfiguration config)
    {
        var bootstrapServers = BootstrapServersParser.Format(config.BootstrapServers);
        _adminClient = new Lazy<IAdminClient>(() =>
            new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build()
        );
    }

    /// <summary>
    /// Ensures a topic exists (idempotent), so a literal-subject consumer's partition assignment — and therefore
    /// readiness — happens deterministically.
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <param name="numPartitions">The partition count to create the topic with when absent.</param>
    /// <returns>A task that completes when the topic exists.</returns>
    public async Task EnsureTopicAsync(string topic, int numPartitions = 1)
    {
        try
        {
            await _adminClient.Value.CreateTopicsAsync([
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = numPartitions,
                    ReplicationFactor = 1,
                },
            ]);
        }
        catch (CreateTopicsException e) when (e.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            // already exists — fine
        }
    }

    /// <summary>
    /// Returns the partitions of a topic (falling back to a single partition when metadata is unavailable).
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <returns>The topic's partitions.</returns>
    public IReadOnlyList<TopicPartition> GetPartitions(string topic)
    {
        var metadata = _adminClient.Value.GetMetadata(topic, TimeSpan.FromSeconds(10));
        var topicMetadata = metadata.Topics.Find(t => t.Topic == topic);
        if (topicMetadata is null || topicMetadata.Partitions.Count == 0)
            return [new TopicPartition(topic, new Partition(0))];

        return topicMetadata.Partitions.Select(p => new TopicPartition(topic, new Partition(p.PartitionId))).ToList();
    }

    /// <summary>
    /// Disposes the admin client, if it was created.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (_adminClient.IsValueCreated)
            _adminClient.Value.Dispose();
    }
}
