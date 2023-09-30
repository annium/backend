using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Server.Models;

public interface IPushContext<TMessage, TState>
    where TMessage : NotificationBase
    where TState : ConnectionStateBase
{
    TState State { get; }
    void Send(TMessage message);
}