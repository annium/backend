namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// Why a load scenario's wait ended — the diagnostic that explains a non-completed run. The gate is strict (it requires
/// a fully-drained, zero-loss run), and the waits are progress-oriented (a generous stall window rather than a short
/// wall-clock), so a slow-but-live broker still reaches <see cref="Completed"/>. A <see cref="Stalled"/> or
/// <see cref="TimedOut"/> outcome therefore means the broker genuinely stopped delivering (a real failure), and this
/// reason is surfaced in the assertion message to say which.
/// </summary>
public enum LoadStopReason
{
    /// <summary>
    /// Every produced message was consumed at least once — the run drained fully (the only passing outcome).
    /// </summary>
    Completed,

    /// <summary>
    /// The overall wall-clock safety fuse elapsed. With progress-oriented waits this should never bind on a healthy
    /// broker; if it does, delivery was pathologically slow or wedged.
    /// </summary>
    TimedOut,

    /// <summary>
    /// Consumption made no progress for the whole stall window while still incomplete — the broker stopped delivering.
    /// </summary>
    Stalled,
}
