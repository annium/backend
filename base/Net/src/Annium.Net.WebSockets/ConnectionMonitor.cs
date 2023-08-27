using System;
using Annium.Debug;

namespace Annium.Net.WebSockets;

public class ConnectionMonitor : IConnectionMonitor
{
    public static IConnectionMonitor None { get; } = new ConnectionMonitor();

    public event Action OnConnectionLost = delegate { };

    private ConnectionMonitor()
    {
    }

    public void Init(ISendingReceivingWebSocket socket)
    {
        this.TraceOld($"Init with {socket.GetFullId()}");
    }

    public void Start()
    {
        this.TraceOld("start");
    }

    public void Stop()
    {
        this.TraceOld("stop");
    }
}