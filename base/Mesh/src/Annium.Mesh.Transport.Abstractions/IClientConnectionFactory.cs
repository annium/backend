using System.Threading.Tasks;

namespace Annium.Mesh.Transport.Abstractions;

public interface IClientConnectionFactory
{
    IClientConnection Create();
}

public interface IClientConnectionFactory<TContext>
{
    Task<IManagedConnection> CreateAsync(TContext context);
}
