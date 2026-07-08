using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Annium.MessageBus.Kafka.Internal;

/// <summary>
/// Kafka admin operations (topic provisioning and partition lookup) over a Confluent <c>AdminClient</c>. Registered as a
/// DI-managed singleton so its client is created once and disposed by the container; consumers resolve it lazily.
/// </summary>
internal interface IKafkaAdmin
{
    /// <summary>
    /// Ensures a topic exists (idempotent), so a literal-subject consumer's partition assignment — and therefore
    /// readiness — happens deterministically.
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <param name="numPartitions">The partition count to create the topic with when absent.</param>
    /// <returns>A task that completes when the topic exists.</returns>
    Task EnsureTopicAsync(string topic, int numPartitions = 1);

    /// <summary>
    /// Returns the partitions of a topic (falling back to a single partition when metadata is unavailable).
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <returns>The topic's partitions.</returns>
    IReadOnlyList<TopicPartition> GetPartitions(string topic);
}
