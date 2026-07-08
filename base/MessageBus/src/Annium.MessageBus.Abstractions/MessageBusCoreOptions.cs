namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Options for the transport-agnostic message-bus core, configured via <c>AddMessageBusCore</c>.
/// </summary>
public sealed class MessageBusCoreOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the registered subscriber also implements <see cref="IReplayableMessageSubscriber"/>
    /// (the same singleton is resolvable as both <see cref="IMessageSubscriber"/> and <see cref="IReplayableMessageSubscriber"/>).
    /// Adapters whose transport supports replay (e.g. Kafka, NATS) opt in; the default is <see langword="false"/>.
    /// </summary>
    public bool SupportsReplay { get; set; }
}
