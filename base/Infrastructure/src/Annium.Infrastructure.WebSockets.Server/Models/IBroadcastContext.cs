using System.Threading;
using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Models
{
    public interface IBroadcastContext<TMessage>
        where TMessage : NotificationBase
    {
        public CancellationToken Token { get; }
        public void Send(TMessage message);
    }
}