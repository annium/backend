using System.Threading;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers;

public interface IBroadcaster<TMessage>
    where TMessage : NotificationBase
{
    public Task Run(IBroadcastContext<TMessage> context, CancellationToken ct);
}