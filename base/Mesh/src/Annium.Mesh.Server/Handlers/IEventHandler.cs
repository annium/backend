using Annium.Core.Mediator;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IEventHandler<TEvent, TState> :
    IFinalRequestHandler<IRequestContext<TEvent, TState>, None>
    where TEvent : EventBase
    where TState : ConnectionStateBase
{
}