using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Mesh.Tests.Base;

public interface IBehavior
{
    static abstract void Register(IServiceContainer container);
    Task RunServer(CancellationToken ct);
}
