using System.Threading;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers
{
    public interface IPusher<TMessage, TState>
        where TMessage : NotificationBase
        where TState : ConnectionStateBase
    {
        public Task RunAsync(IPushContext<TMessage, TState> ctx, CancellationToken ct);
    }
}