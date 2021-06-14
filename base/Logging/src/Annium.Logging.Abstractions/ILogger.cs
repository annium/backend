using System;

namespace Annium.Logging.Abstractions
{
    public interface ILogger
    {
        void Log(ILogSubject? subject, string file, string member, int line, LogLevel level, string message, object[] data);
        void Error(ILogSubject? subject, string file, string member, int line, Exception exception, object[] data);
    }

    public interface ILogger<out T> : ILogger
    {
    }
}