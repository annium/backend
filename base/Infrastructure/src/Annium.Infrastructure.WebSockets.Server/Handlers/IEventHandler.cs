using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers;

public interface IEventHandler<TEvent, TState> :
    IFinalRequestHandler<IRequestContext<TEvent, TState>, Unit>
    where TEvent : EventBase
    where TState : ConnectionStateBase
{
}