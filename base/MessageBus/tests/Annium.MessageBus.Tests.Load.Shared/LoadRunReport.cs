namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// The combined result of a full load run for one broker: the throughput/zero-loss scenario and the ordering scenario.
/// </summary>
/// <param name="BrokerName">The broker display name.</param>
/// <param name="Throughput">The throughput / zero-loss result.</param>
/// <param name="Ordering">The ordering result.</param>
public sealed record LoadRunReport(string BrokerName, ThroughputReport Throughput, OrderingReport Ordering)
{
    /// <summary>
    /// Gets a value indicating whether the run passed its acceptance criteria: zero loss (throughput) and preserved
    /// ordering over a fully-consumed ordering run.
    /// </summary>
    public bool Passed => Throughput.IsZeroLoss && Ordering.IsOrdered && Ordering.IsComplete;
}
