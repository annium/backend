using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

internal class ClientConnectionFactory : IClientConnectionFactory, IClientConnectionFactory<None>
{
    private readonly IConnectionHub _hub;

    public ClientConnectionFactory(IConnectionHub hub)
    {
        _hub = hub;
    }

    public IClientConnection Create()
    {
        var connection = _hub.Create();

        return connection;
    }

    public Task<IManagedConnection> CreateAsync(None context)
    {
        var connection = _hub.Create();

        connection.Connect();

        return Task.FromResult<IManagedConnection>(connection);
    }
}
