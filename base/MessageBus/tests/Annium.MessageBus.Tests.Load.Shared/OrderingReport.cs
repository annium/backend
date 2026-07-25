namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// The result of the ordering scenario (single publisher, single keyed subject, consumer <c>Concurrency=1</c>). In-unit
/// ordering holds when no consumed message has a sequence number less than or equal to a previously consumed one.
/// </summary>
/// <param name="Subject">The subject the run published/consumed on.</param>
/// <param name="Key">The fixed ordering/partition key used for every message.</param>
/// <param name="Produced">The number of messages produced.</param>
/// <param name="ConsumedDistinct">The number of distinct messages consumed (by sequence number).</param>
/// <param name="Duplicates">The number of redelivered (duplicate) messages, excluded from the inversion count.</param>
/// <param name="Inversions">The number of out-of-order (non-increasing) first deliveries.</param>
/// <param name="StopReason">Why the wait ended — <see cref="LoadStopReason.Completed"/> means every produced message
/// was consumed; <see cref="LoadStopReason.TimedOut"/> means the run was cut short (inconclusive completeness).</param>
public sealed record OrderingReport(
    string Subject,
    string Key,
    int Produced,
    int ConsumedDistinct,
    long Duplicates,
    long Inversions,
    LoadStopReason StopReason
)
{
    /// <summary>
    /// Gets a value indicating whether in-unit ordering was preserved (no inversions). This is a volume-independent
    /// signal — it holds over whatever actually arrived, so it is meaningful even on an incomplete run.
    /// </summary>
    public bool IsOrdered => Inversions == 0;

    /// <summary>
    /// Gets a value indicating whether every produced message was consumed (the run completed rather than timing out) —
    /// so a partial, timed-out run cannot masquerade as ordered.
    /// </summary>
    public bool IsComplete => ConsumedDistinct == Produced;

    /// <summary>
    /// Gets a value indicating whether the run drained fully. Equivalent to <see cref="IsComplete"/>, exposed for
    /// symmetry with the throughput report and to key the "assert completeness only on a completed run" gate logic.
    /// </summary>
    public bool Completed => StopReason == LoadStopReason.Completed;
}
