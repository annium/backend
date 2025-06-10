using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client.Internal;

/// <summary>
/// Internal implementation of the client factory for creating mesh client instances
/// </summary>
internal class ClientFactory : IClientFactory
{
    /// <summary>
    /// The time provider for timeout operations
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// The factory for creating client connections
    /// </summary>
    private readonly IClientConnectionFactory _clientConnectionFactory;

    /// <summary>
    /// The serializer for message data
    /// </summary>
    private readonly ISerializer _serializer;

    /// <summary>
    /// The logger for diagnostics
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the ClientFactory class
    /// </summary>
    /// <param name="timeProvider">The time provider for timeout operations</param>
    /// <param name="clientConnectionFactory">The factory for creating client connections</param>
    /// <param name="serializer">The serializer for message data</param>
    /// <param name="logger">The logger for diagnostics</param>
    public ClientFactory(
        ITimeProvider timeProvider,
        IClientConnectionFactory clientConnectionFactory,
        ISerializer serializer,
        ILogger logger
    )
    {
        _timeProvider = timeProvider;
        _clientConnectionFactory = clientConnectionFactory;
        _serializer = serializer;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new client instance with the specified configuration
    /// </summary>
    /// <param name="configuration">The client configuration</param>
    /// <returns>A new client instance</returns>
    public IClient Create(IClientConfiguration configuration)
    {
        var connection = _clientConnectionFactory.Create();

        return new Client(connection, _timeProvider, _serializer, configuration, _logger);
    }

    /// <summary>
    /// Creates a new managed client instance with the specified connection and configuration
    /// </summary>
    /// <param name="connection">The managed connection to use</param>
    /// <param name="configuration">The client configuration</param>
    /// <returns>A new managed client instance</returns>
    public IManagedClient Create(IManagedConnection connection, IClientConfiguration configuration)
    {
        return new ManagedClient(connection, _timeProvider, _serializer, configuration, _logger);
    }
}
