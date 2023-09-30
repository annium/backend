using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Server.Models;

public interface IBroadcastContext<TMessage>
    where TMessage : NotificationBase
{
    public void Send(TMessage message);
}