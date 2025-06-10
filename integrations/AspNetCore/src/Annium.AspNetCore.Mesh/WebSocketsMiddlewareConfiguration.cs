namespace Annium.AspNetCore.Mesh;

/// <summary>
/// Configuration settings for the WebSockets middleware
/// </summary>
public class WebSocketsMiddlewareConfiguration
{
    /// <summary>
    /// Gets or sets the path pattern that the middleware should match
    /// </summary>
    public string PathMatch { get; set; } = string.Empty;
}
