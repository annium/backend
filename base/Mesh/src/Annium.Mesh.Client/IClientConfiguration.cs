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

    /// <summary>
    /// Gets the overall timeout for establishing the initial connection. The underlying transport
    /// retries connection attempts indefinitely; this bounds how long a caller waits for the client
    /// to become connected before the connect fails fast instead of hanging.
    /// </summary>
    Duration ConnectTimeout { get; }
}
