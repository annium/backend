namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// Canonical header keys carried in the transport envelope. Adapters translate these to/from broker-native
/// header/property slots (see the envelope table in the feature spec §8.1). The <c>x-</c> prefix keeps them clear
/// of user headers.
/// </summary>
public static class EnvelopeHeaders
{
    /// <summary>
    /// The message identifier header (mandatory; auto-generated on publish when absent).
    /// </summary>
    public const string Id = "x-msg-id";

    /// <summary>
    /// The logical message type header.
    /// </summary>
    public const string Type = "x-msg-type";

    /// <summary>
    /// The message version header.
    /// </summary>
    public const string Version = "x-msg-version";

    /// <summary>
    /// The payload content-type header.
    /// </summary>
    public const string ContentType = "x-content-type";

    /// <summary>
    /// The publication timestamp header (ISO-8601 round-trip format).
    /// </summary>
    public const string Timestamp = "x-timestamp";

    /// <summary>
    /// The W3C trace-context parent header.
    /// </summary>
    public const string TraceParent = "traceparent";

    /// <summary>
    /// The W3C trace-context state header.
    /// </summary>
    public const string TraceState = "tracestate";

    /// <summary>
    /// The dead-letter reason header (a human-readable summary of why the message was dead-lettered).
    /// </summary>
    public const string DeathReason = "x-death-reason";

    /// <summary>
    /// The dead-letter original-subject header.
    /// </summary>
    public const string OriginalSubject = "x-original-subject";

    /// <summary>
    /// The dead-letter attempt-count header.
    /// </summary>
    public const string Attempts = "x-attempts";

    /// <summary>
    /// The dead-letter first-failure timestamp header (ISO-8601 round-trip format).
    /// </summary>
    public const string FirstFailedAt = "x-first-failed-at";
}
