using System;
using System.Globalization;

namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// Prints a <see cref="LoadRunReport"/> as a human-readable table to the console. The harness has no logging route in a
/// standalone process, so the report is printed directly.
/// </summary>
public static class LoadReportPrinter
{
    /// <summary>
    /// Prints the full run report (both scenarios and the overall result) to <see cref="Console.Out"/>.
    /// </summary>
    /// <param name="report">The run report.</param>
    public static void Print(LoadRunReport report)
    {
        var c = CultureInfo.InvariantCulture;
        var t = report.Throughput;
        var o = report.Ordering;

        Console.WriteLine($"=== Annium.MessageBus Load Report: {report.BrokerName} ===");
        Console.WriteLine();
        Console.WriteLine($"[Throughput / Zero-Loss]  subject={t.Subject}");
        Console.WriteLine($"  Produced:            {t.Produced.ToString("N0", c)}");
        Console.WriteLine(
            $"  Consumed (distinct): {t.ConsumedDistinct.ToString("N0", c)} / {t.Produced.ToString("N0", c)}   -> LOSS: {(t.Produced - t.ConsumedDistinct).ToString("N0", c)}"
        );
        Console.WriteLine($"  Duplicates:          {t.Duplicates.ToString("N0", c)}  (redelivered, deduped)");
        Console.WriteLine($"  Wall time:           {t.Elapsed.TotalSeconds.ToString("F2", c)} s");
        Console.WriteLine($"  Throughput:          {t.MessagesPerSecond.ToString("N0", c)} msg/s");
        Console.WriteLine(
            $"  Latency (ms):        p50={t.Latency.P50.ToString("F2", c)}   p99={t.Latency.P99.ToString("F2", c)}   min={t.Latency.Min.ToString("F2", c)}   max={t.Latency.Max.ToString("F2", c)}   mean={t.Latency.Mean.ToString("F2", c)}   n={t.Latency.Count.ToString("N0", c)}"
        );
        Console.WriteLine();
        Console.WriteLine($"[Ordering]  subject={o.Subject}  key={o.Key}  concurrency=1");
        Console.WriteLine($"  Produced:            {o.Produced.ToString("N0", c)}");
        Console.WriteLine(
            $"  Consumed (distinct): {o.ConsumedDistinct.ToString("N0", c)} / {o.Produced.ToString("N0", c)}"
        );
        Console.WriteLine($"  Duplicates:          {o.Duplicates.ToString("N0", c)}  (excluded from ordering check)");
        Console.WriteLine(
            $"  Inversions:          {o.Inversions.ToString("N0", c)}   -> {(o.IsOrdered ? "ORDER OK" : "OUT OF ORDER")}"
        );
        Console.WriteLine();
        if (report.Passed)
        {
            Console.WriteLine("RESULT: PASS (0-loss, 0 inversions)");
        }
        else
        {
            var reasons = new System.Collections.Generic.List<string>();
            if (!t.IsZeroLoss)
                reasons.Add("throughput loss");
            if (!o.IsComplete)
                reasons.Add("ordering incomplete");
            if (!o.IsOrdered)
                reasons.Add("inversions");
            Console.WriteLine($"RESULT: FAIL ({string.Join(", ", reasons)})");
        }
    }
}
