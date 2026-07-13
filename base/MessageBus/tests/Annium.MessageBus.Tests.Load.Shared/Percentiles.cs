using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// Computes summary latency statistics (mean, min, max, and nearest-rank percentiles) over a sample set. There is no
/// percentile helper elsewhere in the codebase, so this is a small self-contained utility.
/// </summary>
public static class Percentiles
{
    /// <summary>
    /// Computes <see cref="LatencyStats"/> over the given latency samples (milliseconds) using the nearest-rank method
    /// for percentiles.
    /// </summary>
    /// <param name="samples">The latency samples in milliseconds (order irrelevant; not mutated).</param>
    /// <returns>The computed statistics, or <see cref="LatencyStats.Empty"/> when there are no samples.</returns>
    public static LatencyStats Compute(IReadOnlyCollection<double> samples)
    {
        if (samples.Count == 0)
            return LatencyStats.Empty;

        var sorted = samples.ToArray();
        Array.Sort(sorted);

        var sum = 0.0;
        foreach (var s in sorted)
            sum += s;

        return new LatencyStats(
            sorted.Length,
            sum / sorted.Length,
            NearestRank(sorted, 50),
            NearestRank(sorted, 99),
            sorted[0],
            sorted[^1]
        );
    }

    /// <summary>
    /// Returns the value at the given percentile using the nearest-rank method over an ascending-sorted array.
    /// </summary>
    /// <param name="sortedAscending">The samples sorted in ascending order.</param>
    /// <param name="percentile">The percentile in the inclusive range 1..100.</param>
    /// <returns>The value at the requested percentile.</returns>
    private static double NearestRank(double[] sortedAscending, int percentile)
    {
        // rank = ceil(p/100 * n), 1-based; clamp into [1, n].
        var rank = (int)Math.Ceiling(percentile / 100.0 * sortedAscending.Length);
        var index = Math.Clamp(rank - 1, 0, sortedAscending.Length - 1);
        return sortedAscending[index];
    }
}
