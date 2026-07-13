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
public sealed record OrderingReport(
    string Subject,
    string Key,
    int Produced,
    int ConsumedDistinct,
    long Duplicates,
    long Inversions
)
{
    /// <summary>
    /// Gets a value indicating whether in-unit ordering was preserved (no inversions).
    /// </summary>
    public bool IsOrdered => Inversions == 0;

    /// <summary>
    /// Gets a value indicating whether every produced message was consumed (the run completed rather than timing out) —
    /// so a partial, timed-out run cannot masquerade as ordered.
    /// </summary>
    public bool IsComplete => ConsumedDistinct == Produced;
}
