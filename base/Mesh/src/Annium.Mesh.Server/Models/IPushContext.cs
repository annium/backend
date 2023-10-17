using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Models;

public interface IPushContext<TMessage>
    where TMessage : NotificationBase
{
    void Send(TMessage message);
}