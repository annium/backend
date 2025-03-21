using System;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Models;

internal class BroadcastContext<TMessage> : IBroadcastContext<TMessage>
    where TMessage : notnull
{
    private readonly Action<object> _send;

    public BroadcastContext(Action<object> send)
    {
        _send = send;
    }

    public void Send(TMessage message) => _send(message);
}
