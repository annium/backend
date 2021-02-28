using System;
using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Demo.Infrastructure.WebSockets.Domain.Responses.System
{
    public sealed record DiagnosticsNotification : NotificationBase
    {
        public DateTime StartTime { get; init; }
        public long Cpu { get; init; }
        public string Memory { get; init; } = string.Empty;
    }
}