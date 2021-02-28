using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Handlers
{
    public interface IBroadcaster<TMessage>
        where TMessage : NotificationBase
    {
        public Task Run(IBroadcastContext<TMessage> context);
    }
}