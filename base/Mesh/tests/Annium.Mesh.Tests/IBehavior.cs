using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;

namespace Annium.Mesh.Tests;

public interface IBehavior
{
    static abstract void Register(IServiceContainer container);
    Task RunServerAsync(CancellationToken ct);
}
