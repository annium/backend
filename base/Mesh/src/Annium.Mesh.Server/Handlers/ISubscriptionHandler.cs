using Annium.Core.Mediator;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface ISubscriptionHandler<TInit, TMessage, TState> :
    IFinalRequestHandler<ISubscriptionContext<TInit, TMessage, TState>, None>
    where TInit : SubscriptionInitRequestBase
    where TState : ConnectionStateBase
{
}