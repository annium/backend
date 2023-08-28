using System;

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
        this.Trace($"Init with {socket.GetFullId()}");
    }

    public void Start()
    {
        this.Trace("start");
    }

    public void Stop()
    {
        this.Trace("stop");
    }
}