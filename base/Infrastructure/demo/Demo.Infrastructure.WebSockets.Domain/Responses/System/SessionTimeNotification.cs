using System;
using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Demo.Infrastructure.WebSockets.Domain.Responses.System
{
    public sealed class SessionTimeNotification : NotificationBase
    {
        public TimeSpan Duration { get; init; }
    }
}