using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers
{
    public interface IEventHandler<TEvent> :
        IFinalRequestHandler<IRequestContext<TEvent>, Unit>
        where TEvent : EventBase
    {
    }

}