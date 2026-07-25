using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Annium.MessageBus.Abstractions.Internal;

/// <summary>
/// Observability for the shared pipeline built on BCL primitives only (no OpenTelemetry SDK dependency in the
/// library — export is configured by the host application). Exposes a single <see cref="ActivitySource"/> for spans
/// and a single <see cref="Meter"/> for publish/consume/ack/nack/retry/dlq counters plus a consume-latency histogram.
/// </summary>
internal static class Diagnostics
{
    /// <summary>
    /// The instrumentation name shared by the activity source and meter.
    /// </summary>
    public const string Name = "Annium.MessageBus";

    /// <summary>
    /// The activity source for publish/consume spans.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(Name);

    /// <summary>
    /// The meter for message-bus counters and histograms.
    /// </summary>
    private static readonly Meter _meter = new(Name);

    /// <summary>
    /// Counter of published messages.
    /// </summary>
    private static readonly Counter<long> _publishCounter = _meter.CreateCounter<long>("messagebus.publish");

    /// <summary>
    /// Counter of consumed messages (handler invoked).
    /// </summary>
    private static readonly Counter<long> _consumeCounter = _meter.CreateCounter<long>("messagebus.consume");

    /// <summary>
    /// Counter of acknowledged messages.
    /// </summary>
    private static readonly Counter<long> _ackCounter = _meter.CreateCounter<long>("messagebus.ack");

    /// <summary>
    /// Counter of nacked messages.
    /// </summary>
    private static readonly Counter<long> _nackCounter = _meter.CreateCounter<long>("messagebus.nack");

    /// <summary>
    /// Counter of retry attempts.
    /// </summary>
    private static readonly Counter<long> _retryCounter = _meter.CreateCounter<long>("messagebus.retry");

    /// <summary>
    /// Counter of dead-lettered messages.
    /// </summary>
    private static readonly Counter<long> _dlqCounter = _meter.CreateCounter<long>("messagebus.dlq");

    /// <summary>
    /// Histogram of consume latency in milliseconds.
    /// </summary>
    private static readonly Histogram<double> _consumeLatency = _meter.CreateHistogram<double>(
        "messagebus.consume.latency",
        unit: "ms"
    );

    /// <summary>
    /// Starts a producer span for the given subject.
    /// </summary>
    /// <param name="subject">The destination subject.</param>
    /// <returns>The started activity, or null when no listener is attached.</returns>
    public static Activity? StartPublish(string subject) =>
        ActivitySource.StartActivity($"{subject} publish", ActivityKind.Producer);

    /// <summary>
    /// Starts a consumer span for the given subject and message.
    /// </summary>
    /// <param name="subject">The source subject.</param>
    /// <param name="id">The message identifier.</param>
    /// <returns>The started activity, or null when no listener is attached.</returns>
    public static Activity? StartConsume(string subject, string id)
    {
        var activity = ActivitySource.StartActivity($"{subject} consume", ActivityKind.Consumer);
        activity?.SetTag("messaging.message.id", id);
        return activity;
    }

    /// <summary>
    /// Records a published message.
    /// </summary>
    /// <param name="subject">The destination subject.</param>
    public static void RecordPublish(string subject) => _publishCounter.Add(1, Tag(subject));

    /// <summary>
    /// Records a consumed message (handler invoked).
    /// </summary>
    /// <param name="subject">The source subject.</param>
    public static void RecordConsume(string subject) => _consumeCounter.Add(1, Tag(subject));

    /// <summary>
    /// Records an acknowledgement.
    /// </summary>
    /// <param name="subject">The source subject.</param>
    public static void RecordAck(string subject) => _ackCounter.Add(1, Tag(subject));

    /// <summary>
    /// Records a rejection.
    /// </summary>
    /// <param name="subject">The source subject.</param>
    public static void RecordNack(string subject) => _nackCounter.Add(1, Tag(subject));

    /// <summary>
    /// Records a retry attempt.
    /// </summary>
    /// <param name="subject">The source subject.</param>
    public static void RecordRetry(string subject) => _retryCounter.Add(1, Tag(subject));

    /// <summary>
    /// Records a dead-lettered message.
    /// </summary>
    /// <param name="subject">The source subject.</param>
    public static void RecordDlq(string subject) => _dlqCounter.Add(1, Tag(subject));

    /// <summary>
    /// Records consume latency for a message.
    /// </summary>
    /// <param name="subject">The source subject.</param>
    /// <param name="milliseconds">The elapsed handler time in milliseconds.</param>
    public static void RecordConsumeLatency(string subject, double milliseconds) =>
        _consumeLatency.Record(milliseconds, Tag(subject));

    /// <summary>
    /// Builds the standard subject tag for an instrument measurement.
    /// </summary>
    /// <param name="subject">The subject value.</param>
    /// <returns>The tag key/value pair.</returns>
    private static KeyValuePair<string, object?> Tag(string subject) => new("messaging.destination.name", subject);
}
