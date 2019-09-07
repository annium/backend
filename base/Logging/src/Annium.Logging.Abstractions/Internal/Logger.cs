using System;

namespace Annium.Logging.Abstractions
{
    internal class Logger<T> : ILogger<T>
    {
        private readonly LogRouter router;

        public Logger(
            LogRouter router
        )
        {
            this.router = router;
        }

        public void Log(LogLevel level, string message) => router.Send(level, typeof(T), message, null);

        public void Trace(string message) => router.Send(LogLevel.Trace, typeof(T), message, null);

        public void Debug(string message) => router.Send(LogLevel.Debug, typeof(T), message, null);

        public void Info(string message) => router.Send(LogLevel.Info, typeof(T), message, null);

        public void Warn(string message) => router.Send(LogLevel.Warn, typeof(T), message, null);

        public void Error(Exception exception) => router.Send(LogLevel.Error, typeof(T), exception.Message, exception);

        public void Error(string message) => router.Send(LogLevel.Error, typeof(T), message, null);
    }
}