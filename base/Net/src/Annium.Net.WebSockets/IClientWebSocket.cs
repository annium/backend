using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets
{
    public interface IClientWebSocket : ISendingWebSocket, IReceivingWebSocket, IDisposable
    {
        Task ConnectAsync(Uri uri, CancellationToken token);
        Task DisconnectAsync(CancellationToken token);
    }
}