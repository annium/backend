using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IPusher<TMessage>
{
    public Task RunAsync(IPushContext<TMessage> ctx, CancellationToken ct);
}