using System;
using System.Globalization;

namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// The knobs for a load run — both the throughput/zero-loss scenario and the ordering scenario. All fields have
/// production-representative defaults; presets (<see cref="Default"/>, <see cref="Small"/>) and simple <c>key=value</c>
/// argument parsing (<see cref="Parse"/>) let callers scale a run up or down.
/// </summary>
public sealed record LoadScenarioOptions
{
    /// <summary>Gets the number of concurrent publishers in the throughput scenario.</summary>
    public int Publishers { get; init; } = 4;

    /// <summary>Gets the number of messages each publisher sends in the throughput scenario.</summary>
    public int MessagesPerPublisher { get; init; } = 25_000;

    /// <summary>Gets the subject for the throughput scenario.</summary>
    public string ThroughputSubject { get; init; } = "load.throughput";

    /// <summary>Gets the delivery window (max unacknowledged in-flight) for the throughput subscriber.</summary>
    public int ThroughputPrefetch { get; init; } = 256;

    /// <summary>Gets the number of concurrent handlers for the throughput subscriber.</summary>
    public int ThroughputConcurrency { get; init; } = 8;

    /// <summary>Gets the number of messages published per <c>PublishBatchAsync</c> call.</summary>
    public int BatchSize { get; init; } = 200;

    /// <summary>
    /// Gets the maximum number of publish calls issued concurrently. Zero means "equal to <see cref="Publishers"/>"
    /// (full concurrency). Adapters whose producer is not safe under concurrent publishing (RabbitMQ shares one publish
    /// channel) set this to 1 to serialize issuance.
    /// </summary>
    public int MaxPublisherConcurrency { get; init; }

    /// <summary>Gets the overall timeout for the throughput scenario.</summary>
    public TimeSpan ThroughputTimeout { get; init; } = TimeSpan.FromSeconds(120);

    /// <summary>Gets the no-progress (stall) timeout after which the throughput scenario aborts early.</summary>
    public TimeSpan StallTimeout { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>Gets the number of messages in the ordering scenario.</summary>
    public int OrderingMessages { get; init; } = 5_000;

    /// <summary>Gets the subject for the ordering scenario.</summary>
    public string OrderingSubject { get; init; } = "load.ordering";

    /// <summary>Gets the fixed partition/ordering key used in the ordering scenario (pins Kafka to one partition).</summary>
    public string OrderingKey { get; init; } = "ordering-unit";

    /// <summary>
    /// Gets the overall timeout for the ordering scenario. Generous because the ordering scenario publishes strictly one
    /// message at a time, awaited (to keep producer-side ordering unambiguous) — on Kafka each awaited produce incurs the
    /// producer's linger delay, so sequential publishing is deliberately slow.
    /// </summary>
    public TimeSpan OrderingTimeout { get; init; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Gets the effective maximum publish concurrency (<see cref="MaxPublisherConcurrency"/>, or <see cref="Publishers"/>
    /// when unset/zero).
    /// </summary>
    public int EffectiveMaxPublisherConcurrency => MaxPublisherConcurrency > 0 ? MaxPublisherConcurrency : Publishers;

    /// <summary>Gets the total number of messages produced in the throughput scenario.</summary>
    public int ThroughputTotal => Publishers * MessagesPerPublisher;

    /// <summary>
    /// Gets the default (full-scale) preset: 4 publishers × 25,000 = 100,000 throughput messages, 5,000 ordering.
    /// </summary>
    public static LoadScenarioOptions Default { get; } = new();

    /// <summary>
    /// Gets the small preset for the in-CI zero-loss gate: 4 × 1,250 = 5,000 throughput messages, 2,000 ordering, with
    /// shorter timeouts.
    /// </summary>
    public static LoadScenarioOptions Small { get; } =
        new()
        {
            MessagesPerPublisher = 1_250,
            OrderingMessages = 2_000,
            ThroughputTimeout = TimeSpan.FromSeconds(60),
            StallTimeout = TimeSpan.FromSeconds(15),
            OrderingTimeout = TimeSpan.FromSeconds(30),
        };

    /// <summary>
    /// Parses <c>key=value</c> overrides from command-line arguments onto the <see cref="Default"/> preset. Recognized
    /// keys: <c>publishers</c>, <c>messages</c> (per publisher), <c>prefetch</c>, <c>concurrency</c>, <c>batch</c>,
    /// <c>maxpub</c>, <c>ordering</c> (ordering message count). Unknown keys are ignored.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The resolved options.</returns>
    public static LoadScenarioOptions Parse(string[] args)
    {
        var o = Default;
        foreach (var arg in args)
        {
            var parts = arg.TrimStart('-').Split('=', 2);
            if (
                parts.Length != 2
                || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            )
                continue;

            o = parts[0].ToLowerInvariant() switch
            {
                "publishers" => o with { Publishers = v },
                "messages" => o with { MessagesPerPublisher = v },
                "prefetch" => o with { ThroughputPrefetch = v },
                "concurrency" => o with { ThroughputConcurrency = v },
                "batch" => o with { BatchSize = v },
                "maxpub" => o with { MaxPublisherConcurrency = v },
                "ordering" => o with { OrderingMessages = v },
                _ => o,
            };
        }

        return o;
    }
}
