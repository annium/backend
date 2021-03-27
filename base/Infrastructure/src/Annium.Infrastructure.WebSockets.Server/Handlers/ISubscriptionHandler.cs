using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers
{
    public interface ISubscriptionHandler<TInit, TMessage, TState> :
        IFinalRequestHandler<ISubscriptionContext<TInit, TMessage, TState>, Unit>
        where TInit : SubscriptionInitRequestBase
        where TState : ConnectionStateBase
    {
    }
}