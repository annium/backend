using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Internal
{
    internal interface IKeepAliveMonitor : IAsyncDisposable
    {
        CancellationToken Token { get; }
        void Resume();
        Task PauseAsync();
    }
}