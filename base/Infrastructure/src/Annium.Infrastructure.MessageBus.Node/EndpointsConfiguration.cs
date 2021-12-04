namespace Annium.Infrastructure.MessageBus.Node;

public record EndpointsConfiguration
{
    public string PubEndpoint { get; set; } = string.Empty;
    public string SubEndpoint { get; set; } = string.Empty;
}