namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// The payload published by the load harness. Carries a globally-unique monotonic sequence number (for dedup and
/// ordering checks) and the publish timestamp used to compute end-to-end latency.
/// </summary>
/// <param name="Seq">The globally-unique message sequence number.</param>
/// <param name="PublishedTimestamp">The publish time as a high-resolution <see cref="System.Diagnostics.Stopwatch.GetTimestamp"/>
/// tick count (NOT wall-clock ticks). Valid only within the single process that both publishes and consumes, where
/// <see cref="System.Diagnostics.Stopwatch.GetElapsedTime(long)"/> yields skew-free latency.</param>
public sealed record LoadMessage(long Seq, long PublishedTimestamp);
