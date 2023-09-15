using System;

namespace Annium.Internal.Logging;

internal static class LogConfig
{
    public static readonly string Level;

    static LogConfig()
    {
        var raw = Environment.GetEnvironmentVariable("ANNIUM_LOG");

        Level = raw?.Trim() ?? string.Empty;
    }
}