using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Models
{
    public interface IPushContext<TMessage, TState>
        where TMessage : NotificationBase
        where TState : ConnectionStateBase
    {
        TState State { get; }
        void Send(TMessage message);
    }
}