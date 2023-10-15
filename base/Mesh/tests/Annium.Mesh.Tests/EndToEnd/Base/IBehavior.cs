using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Tests.System.Client.Clients;

namespace Annium.Mesh.Tests.EndToEnd.Base;

public interface IBehavior
{
    static abstract void Register(IServiceContainer container);
    Task RunServer(CancellationToken ct);
    Task<TestServerManagedClient> GetClient();
}