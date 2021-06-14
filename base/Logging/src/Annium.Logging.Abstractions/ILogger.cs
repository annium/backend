using System;

namespace Annium.Logging.Abstractions
{
    public interface ILogger
    {
        void Log(ILogSubject subject, LogLevel level, string message, object[] data);
        void Trace(ILogSubject subject, string message, object[] data);
        void Debug(ILogSubject subject, string message, object[] data);
        void Info(ILogSubject subject, string message, object[] data);
        void Warn(ILogSubject subject, string message, object[] data);
        void Error(ILogSubject subject, Exception exception, object[] data);
        void Error(ILogSubject subject, string message, object[] data);
    }

    public interface ILogger<out T> : ILogger
    {
    }
}