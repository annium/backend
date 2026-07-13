using System.Linq;
using Annium.MessageBus.Tests.Load.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// Pure unit tests for the load harness's <see cref="Percentiles"/> utility (no broker).
/// </summary>
public sealed class PercentilesTests
{
    /// <summary>
    /// An empty sample set yields the empty statistics.
    /// </summary>
    [Fact]
    public void Compute_Empty_ReturnsEmpty()
    {
        var stats = Percentiles.Compute([]);
        stats.Count.Is(0);
        stats.P50.Is(0);
        stats.P99.Is(0);
    }

    /// <summary>
    /// Over 1..100, nearest-rank p50=50, p99=99, plus min/max/mean/count.
    /// </summary>
    [Fact]
    public void Compute_Hundred_NearestRankPercentiles()
    {
        var samples = Enumerable.Range(1, 100).Select(i => (double)i).ToArray();

        var stats = Percentiles.Compute(samples);

        stats.Count.Is(100);
        stats.Min.Is(1);
        stats.Max.Is(100);
        stats.Mean.Is(50.5);
        // nearest-rank: rank = ceil(p/100 * n); p50 -> index 49 (value 50), p99 -> index 98 (value 99)
        stats.P50.Is(50);
        stats.P99.Is(99);
    }

    /// <summary>
    /// Percentiles are order-independent (input is sorted internally).
    /// </summary>
    [Fact]
    public void Compute_Unsorted_SortsInternally()
    {
        var stats = Percentiles.Compute([5, 1, 4, 2, 3]);
        stats.Min.Is(1);
        stats.Max.Is(5);
        stats.P50.Is(3);
    }
}
