using System;
using System.Net.WebSockets;

namespace Annium.Net.WebSockets
{
    public readonly struct SocketMessage
    {
        public WebSocketMessageType Type { get; }
        public ReadOnlyMemory<byte> Data { get; }

        public SocketMessage(
            WebSocketMessageType type,
            ReadOnlyMemory<byte> data
        )
        {
            Type = type;
            Data = data;
        }
    }
}