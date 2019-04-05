using System;

namespace Annium.Logging.Abstractions
{
    public interface ILogger
    {
        void Log(LogLevel level, string message);

        void Trace(string message);

        void Debug(string message);

        void Info(string message);

        void Warn(string message);

        void Error(Exception exception);

        void Error(string message);
    }

    public interface ILogger<T> : ILogger where T : class { }
}