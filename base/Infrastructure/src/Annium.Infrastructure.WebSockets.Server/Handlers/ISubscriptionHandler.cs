using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers
{
    public interface ISubscriptionHandler<TInit, TMessage> :
        IFinalRequestHandler<ISubscriptionContext<TInit, TMessage>, Unit>
        where TInit : SubscriptionInitRequestBase
    {
    }
}