using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IBroadcaster<TMessage>
    where TMessage : NotificationBase
{
    public Task Run(IBroadcastContext<TMessage> context, CancellationToken ct);
}