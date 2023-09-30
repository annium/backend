using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Models;

public interface IBroadcastContext<TMessage>
    where TMessage : NotificationBase
{
    public void Send(TMessage message);
}