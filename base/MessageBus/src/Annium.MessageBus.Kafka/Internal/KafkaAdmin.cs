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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IReadOnlyList<TopicPartition> GetPartitions(string topic)
    {
        var metadata = _adminClient.Value.GetMetadata(topic, TimeSpan.FromSeconds(10));
        var topicMetadata = metadata.Topics.Find(t => t.Topic == topic);
        if (topicMetadata is null || topicMetadata.Partitions.Count == 0)
            return [new TopicPartition(topic, new Partition(0))];

        return topicMetadata.Partitions.Select(p => new TopicPartition(topic, new Partition(p.PartitionId))).ToList();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (_adminClient.IsValueCreated)
            _adminClient.Value.Dispose();
    }
}
