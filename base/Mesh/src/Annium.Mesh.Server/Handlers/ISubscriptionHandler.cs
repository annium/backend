using Annium.Core.Mediator;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface ISubscriptionHandler<TInit, TMessage> :
    IFinalRequestHandler<ISubscriptionContext<TInit, TMessage>, None>
    where TInit : SubscriptionInitRequestBase
{
}