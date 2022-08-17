using System;

namespace Annium.Logging.Abstractions;

public interface ILogger<out T>
{
    void Log<TS>(TS? subject, string file, string member, int line, LogLevel level, string message, object[] data);
    void Error<TS>(TS? subject, string file, string member, int line, Exception exception, object[] data);
}