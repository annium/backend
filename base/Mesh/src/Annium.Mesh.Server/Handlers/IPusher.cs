using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IPusher<TMessage>
    where TMessage : NotificationBaseObsolete
{
    public Task RunAsync(IPushContext<TMessage> ctx, CancellationToken ct);
}