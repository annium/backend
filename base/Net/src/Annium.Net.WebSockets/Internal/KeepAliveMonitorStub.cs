using System.Threading;

namespace Annium.Net.WebSockets.Internal;

internal class KeepAliveMonitorStub : IKeepAliveMonitor
{
    public CancellationToken Token => CancellationToken.None;

    public void Resume()
    {
    }

    public void Pause()
    {
    }
}