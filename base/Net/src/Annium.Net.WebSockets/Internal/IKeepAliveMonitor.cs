using System.Threading;

namespace Annium.Net.WebSockets.Internal
{
    internal interface IKeepAliveMonitor
    {
        CancellationToken Token { get; }
        void Resume();
        void Pause();
    }
}