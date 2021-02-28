using System.Threading;
using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public interface IBroadcastContext<TMessage>
        where TMessage : NotificationBase
    {
        public CancellationToken Token { get; }
        public void Send(TMessage message);
    }
}