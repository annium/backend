namespace Annium.MessageBus.Abstractions.Internal;

/// <summary>
/// The acknowledgement disposition recorded by a handler for a single processing attempt. The pipeline enforces
/// that exactly one of <see cref="Ack"/> / <see cref="Nack"/> is set; leaving it at <see cref="None"/> is a
/// contract violation.
/// </summary>
internal enum Disposition
{
    /// <summary>
    /// No disposition recorded yet (handler has not acked or nacked).
    /// </summary>
    None,

    /// <summary>
    /// The message was acknowledged (successful processing).
    /// </summary>
    Ack,

    /// <summary>
    /// The message was rejected (retry-then-dead-letter, or immediate dead-letter).
    /// </summary>
    Nack,
}
