namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// Summary latency statistics (all in milliseconds) computed over the per-message latency samples of a throughput run.
/// </summary>
/// <param name="Count">The number of latency samples.</param>
/// <param name="Mean">The arithmetic mean latency (ms).</param>
/// <param name="P50">The median (50th percentile) latency (ms).</param>
/// <param name="P99">The 99th percentile latency (ms).</param>
/// <param name="Min">The minimum observed latency (ms).</param>
/// <param name="Max">The maximum observed latency (ms).</param>
public sealed record LatencyStats(int Count, double Mean, double P50, double P99, double Min, double Max)
{
    /// <summary>
    /// Gets an empty statistics value (no samples).
    /// </summary>
    public static LatencyStats Empty { get; } = new(0, 0, 0, 0, 0, 0);
}
