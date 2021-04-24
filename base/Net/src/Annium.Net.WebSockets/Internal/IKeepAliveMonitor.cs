using System;
using System.Threading;

namespace Annium.Net.WebSockets.Internal
{
    internal interface IKeepAliveMonitor : IAsyncDisposable
    {
        CancellationToken Token { get; }
        void Resume();
        void Pause();
    }
}