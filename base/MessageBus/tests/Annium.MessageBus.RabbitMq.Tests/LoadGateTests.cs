using System.Threading.Tasks;
using Annium.MessageBus.Tests.Load.Shared;
using Annium.MessageBus.Tests.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// Small-scale zero-loss / ordering gate for the RabbitMQ adapter: runs the shared load harness at a reduced volume so
/// CI catches a zero-loss or ordering regression without a full load run. Publish concurrency is 1 — the adapter shares
/// a single publish channel, which is not safe under concurrent publishing.
/// </summary>
public sealed class LoadGateTests : MessageBusConformanceTestBase<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadGateTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public LoadGateTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// The small-scale load run is zero-loss and preserves ordering.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Load_SmallScale_ZeroLossAndOrdered()
    {
        var harness = new LoadHarness(Publisher, Subscriber, "RabbitMq");
        var options = LoadScenarioOptions.Small with { MaxPublisherConcurrency = 1 };
        var report = await harness.RunAsync(options, TestContext.Current.CancellationToken);

        // Strict gate: the run must fully drain with zero loss and preserved ordering. The wait is progress-oriented
        // (StallTimeout, not a short wall-clock), so a slow-but-live broker still completes — an incomplete run means
        // the broker actually stopped delivering, which is a real failure. StopReason explains why on failure.
        var t = report.Throughput;
        t.IsZeroLoss.Is(true, $"throughput not zero-loss ({t.StopReason}): consumed {t.ConsumedDistinct}/{t.Produced}");

        var o = report.Ordering;
        o.IsComplete.Is(true, $"ordering incomplete ({o.StopReason}): consumed {o.ConsumedDistinct}/{o.Produced}");
        o.IsOrdered.Is(true, $"ordering inversions: {o.Inversions}");
    }
}
