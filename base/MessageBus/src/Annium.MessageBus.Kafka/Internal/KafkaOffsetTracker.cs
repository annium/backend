using System.Collections.Generic;
using Confluent.Kafka;

namespace Annium.MessageBus.Kafka.Internal;

/// <summary>
/// Tracks per-partition completed offsets and computes the largest contiguous prefix that is safe to store (commit)
/// under at-least-once. With <c>Concurrency &gt; 1</c> deliveries complete out of order, so an offset may only be
/// committed once every lower offset on its partition has also completed (an ack that dead-letters counts as
/// completed, closing the gap). See feature spec §8.2.1. Not thread-safe; callers serialize access (the consumer lock).
/// </summary>
internal sealed class KafkaOffsetTracker
{
    /// <summary>
    /// Per-partition progress state.
    /// </summary>
    private sealed class PartitionState
    {
        /// <summary>
        /// The next offset to store: every offset below it on this partition has completed.
        /// </summary>
        public long Next;

        /// <summary>
        /// Completed offsets at or above <see cref="Next"/>, awaiting a contiguous run.
        /// </summary>
        public readonly HashSet<long> Completed = new();

        /// <summary>
        /// Whether <see cref="Next"/> has been seeded from the first delivered offset.
        /// </summary>
        public bool Seeded;
    }

    /// <summary>
    /// State per topic-partition.
    /// </summary>
    private readonly Dictionary<TopicPartition, PartitionState> _states = new();

    /// <summary>
    /// Records that an offset was delivered to a handler, seeding the partition's contiguous cursor from the first
    /// (lowest, since Kafka delivers in order per partition) delivered offset. Idempotent for redelivered offsets.
    /// </summary>
    /// <param name="tp">The topic-partition.</param>
    /// <param name="offset">The delivered offset.</param>
    public void OnDelivered(TopicPartition tp, long offset)
    {
        var state = GetState(tp);
        if (!state.Seeded)
        {
            state.Next = offset;
            state.Seeded = true;
        }
    }

    /// <summary>
    /// Marks an offset completed (acked, or dead-lettered). Returns the next offset to store when the contiguous
    /// prefix advanced, or <see langword="null"/> when it did not (a lower offset is still outstanding).
    /// </summary>
    /// <param name="tp">The topic-partition.</param>
    /// <param name="offset">The completed offset.</param>
    /// <returns>The offset to store (contiguous prefix + 1), or null.</returns>
    public long? Complete(TopicPartition tp, long offset)
    {
        var state = GetState(tp);
        if (!state.Seeded)
        {
            state.Next = offset;
            state.Seeded = true;
        }

        if (offset < state.Next)
            return null; // already accounted for (e.g. redelivered then completed later)

        state.Completed.Add(offset);

        var advanced = false;
        while (state.Completed.Remove(state.Next))
        {
            state.Next++;
            advanced = true;
        }

        return advanced ? state.Next : null;
    }

    /// <summary>
    /// Gets (creating if needed) the state for a topic-partition.
    /// </summary>
    /// <param name="tp">The topic-partition.</param>
    /// <returns>The partition state.</returns>
    private PartitionState GetState(TopicPartition tp)
    {
        if (!_states.TryGetValue(tp, out var state))
            _states[tp] = state = new PartitionState();
        return state;
    }
}
