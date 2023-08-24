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
    }

    public void Start()
    {
    }

    public void Stop()
    {
    }
}