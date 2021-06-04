using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Infrastructure.WebSockets.Client
{
    public interface IClient : IClientBase
    {
        event Func<Task> ConnectionLost;
        event Func<Task> ConnectionRestored;
        Task ConnectAsync(CancellationToken ct = default);
        Task DisconnectAsync();
    }
}