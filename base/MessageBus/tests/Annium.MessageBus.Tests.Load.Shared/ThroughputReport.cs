using System;

namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// The result of the throughput / zero-loss scenario. Zero-loss is the acceptance threshold; throughput and latency are
/// recorded as a baseline with no hard SLA.
/// </summary>
/// <param name="Subject">The subject the run published/consumed on.</param>
/// <param name="Produced">The number of messages produced.</param>
/// <param name="ConsumedDistinct">The number of distinct messages consumed (by sequence number).</param>
/// <param name="Duplicates">The number of redelivered (duplicate) messages, deduplicated and not counted as consumed.</param>
/// <param name="Elapsed">The wall-clock time from first publish to full consumption (or timeout).</param>
/// <param name="MessagesPerSecond">The throughput baseline (distinct consumed / elapsed seconds).</param>
/// <param name="Latency">The end-to-end latency statistics.</param>
public sealed record ThroughputReport(
    string Subject,
    int Produced,
    int ConsumedDistinct,
    long Duplicates,
    TimeSpan Elapsed,
    double MessagesPerSecond,
    LatencyStats Latency
)
{
    /// <summary>
    /// Gets a value indicating whether every produced message was consumed at least once (zero loss).
    /// </summary>
    public bool IsZeroLoss => ConsumedDistinct == Produced;
}
