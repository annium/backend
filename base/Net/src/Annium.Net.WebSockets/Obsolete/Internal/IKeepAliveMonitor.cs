using System;
using System.Threading;

namespace Annium.Net.WebSockets.Obsolete.Internal;

[Obsolete]
internal interface IKeepAliveMonitor
{
    CancellationToken Token { get; }
    void Resume();
    void Pause();
}