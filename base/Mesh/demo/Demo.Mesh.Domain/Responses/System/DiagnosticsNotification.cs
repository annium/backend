using System;
using Annium.Mesh.Domain.Responses;

namespace Demo.Mesh.Domain.Responses.System;

public sealed class DiagnosticsNotification : NotificationBase
{
    public DateTime StartTime { get; init; }
    public long Cpu { get; init; }
    public string Memory { get; init; } = string.Empty;
}