namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Subscription settings for replay-capable transports, adding a start position. Accepted only by
/// <see cref="ISupportsReplay"/>.
/// </summary>
public sealed record ReplaySubscriptionOptions : SubscriptionOptions
{
    /// <summary>
    /// Gets the position to start consuming from. Defaults to <see cref="StartPosition.New"/>.
    /// </summary>
    public StartPosition StartPosition { get; init; } = StartPosition.New;
}
