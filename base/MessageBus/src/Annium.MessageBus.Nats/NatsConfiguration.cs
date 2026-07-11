using System;

namespace Annium.MessageBus.Nats;

/// <summary>
/// The resolved NATS adapter configuration built by <see cref="INatsConfigurationBuilder"/>. A plain DTO holding the
/// validated server URL (validation/parsing lives in the builder).
/// </summary>
public sealed record NatsConfiguration
{
    /// <summary>
    /// Gets the NATS server URL (<c>nats://</c> or <c>tls://</c>, optionally a comma-separated seed list).
    /// </summary>
    public required Uri Url { get; init; }
}
