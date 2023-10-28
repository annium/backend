using System.Threading.Tasks;

namespace Annium.Mesh.Transport.Abstractions;

public interface IServerConnectionFactory<TContext>
{
    Task<IServerConnection> CreateAsync(TContext context);
}
