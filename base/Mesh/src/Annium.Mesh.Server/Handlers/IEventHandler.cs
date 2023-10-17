using Annium.Core.Mediator;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IEventHandler<TEvent> :
    IFinalRequestHandler<IRequestContext<TEvent>, None>
    where TEvent : EventBase
{
}