namespace Annium.Infrastructure.MessageBus.Node;

/// <summary>
/// Configuration for MessageBus network endpoints defining publisher and subscriber connection points.
/// </summary>
public record EndpointsConfiguration
{
    /// <summary>
    /// Gets or sets the publisher endpoint address for sending messages.
    /// </summary>
    public string PubEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscriber endpoint address for receiving messages.
    /// </summary>
    public string SubEndpoint { get; set; } = string.Empty;
}
