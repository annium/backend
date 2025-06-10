using NodaTime;

namespace Annium.Mesh.Client;

/// <summary>
/// Configuration interface for mesh client settings
/// </summary>
public interface IClientConfiguration
{
    /// <summary>
    /// Gets the timeout duration for client requests
    /// </summary>
    Duration ResponseTimeout { get; }
}
