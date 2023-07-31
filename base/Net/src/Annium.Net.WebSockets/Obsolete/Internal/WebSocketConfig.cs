using System;

namespace Annium.Net.WebSockets.Obsolete.Internal;

[Obsolete]
internal record struct WebSocketConfig
{
    public required bool ResumeImmediately { get; init; }
}