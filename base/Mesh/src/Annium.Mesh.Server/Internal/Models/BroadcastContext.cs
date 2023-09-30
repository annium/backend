using System;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models;

internal class BroadcastContext<TMessage> : IBroadcastContext<TMessage>
    where TMessage : NotificationBase
{
    private readonly Action<object> _send;

    public BroadcastContext(
        Action<object> send
    )
    {
        _send = send;
    }

    public void Send(TMessage message) => _send(message);
}