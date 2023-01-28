namespace Annium.Net.WebSockets.Internal;

internal record struct WebSocketConfig
{
    public required bool ResumeImmediately { get; init; }
}