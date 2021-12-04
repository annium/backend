using System;

namespace Annium.Logging.Shared;

public record LogRouteConfiguration
{
    public TimeSpan BufferTime { get; init; } = TimeSpan.FromMinutes(1);
    public int BufferCount { get; init; } = 100;
}