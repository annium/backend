using Annium.Logging;

namespace Annium.Mesh.Client.Internal;

internal class ClientFactory : IClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly Serializer _serializer;
    private readonly ILogger _logger;

    public ClientFactory(
        ITimeProvider timeProvider,
        Serializer serializer,
        ILogger logger
    )
    {
        _timeProvider = timeProvider;
        _serializer = serializer;
        _logger = logger;
    }

    public IClient Create(IClientConfiguration configuration)
    {
        return new Client(_timeProvider, _serializer, configuration, _logger);
    }
}