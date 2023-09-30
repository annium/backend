using System;
using Annium.Mesh.Domain.Responses;

namespace Demo.Mesh.Domain.Responses.System;

public sealed class SessionTimeNotification : NotificationBase
{
    public TimeSpan Duration { get; init; }
}