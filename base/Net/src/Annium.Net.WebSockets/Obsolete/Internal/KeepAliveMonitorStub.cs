using System;
using System.Threading;

namespace Annium.Net.WebSockets.Obsolete.Internal;

[Obsolete]
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