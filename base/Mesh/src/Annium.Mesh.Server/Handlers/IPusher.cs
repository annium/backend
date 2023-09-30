using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IPusher<TMessage, TState>
    where TMessage : NotificationBase
    where TState : ConnectionStateBase
{
    public Task RunAsync(IPushContext<TMessage, TState> ctx, CancellationToken ct);
}