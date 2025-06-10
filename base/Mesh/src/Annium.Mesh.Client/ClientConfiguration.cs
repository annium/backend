using NodaTime;

namespace Annium.Mesh.Client;

/// <summary>
/// Default implementation of client configuration with fluent configuration methods
/// </summary>
public class ClientConfiguration : IClientConfiguration
{
    /// <summary>
    /// Gets the timeout duration for client requests
    /// </summary>
    public Duration ResponseTimeout { get; private set; } = Duration.FromMinutes(1);

    /// <summary>
    /// Sets the response timeout for client requests
    /// </summary>
    /// <param name="timeout">The timeout in seconds</param>
    /// <returns>The current configuration instance for fluent chaining</returns>
    public ClientConfiguration WithResponseTimeout(uint timeout)
    {
        ResponseTimeout = Duration.FromSeconds(timeout);

        return this;
    }
}
