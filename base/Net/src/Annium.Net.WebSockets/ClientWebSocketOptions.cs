namespace Annium.Net.WebSockets
{
    public record ClientWebSocketOptions : WebSocketBaseOptions
    {
        public bool ReconnectOnFailure { get; set; }
    }
}