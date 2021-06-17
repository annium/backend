using System;

namespace Annium.Logging.Abstractions
{
    public interface ILogger
    {
        void Log<T>(T? subject, string file, string member, int line, LogLevel level, string message, object[] data) where T : class, ILogSubject;
        void Error<T>(T? subject, string file, string member, int line, Exception exception, object[] data) where T : class, ILogSubject;
    }

    public interface ILogger<out T> : ILogger
    {
    }
}