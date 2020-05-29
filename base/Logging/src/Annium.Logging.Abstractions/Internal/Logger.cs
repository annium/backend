using System;

namespace Annium.Logging.Abstractions.Internal
{
    internal class Logger<T> : ILogger<T>
    {
        private readonly ILogRouter _router;

        public Logger(
            ILogRouter router
        )
        {
            this._router = router;
        }

        public void Log(LogLevel level, string message) => _router.Send(level, typeof(T), message, null);

        public void Trace(string message) => _router.Send(LogLevel.Trace, typeof(T), message, null);

        public void Debug(string message) => _router.Send(LogLevel.Debug, typeof(T), message, null);

        public void Info(string message) => _router.Send(LogLevel.Info, typeof(T), message, null);

        public void Warn(string message) => _router.Send(LogLevel.Warn, typeof(T), message, null);

        public void Error(Exception exception) => _router.Send(LogLevel.Error, typeof(T), exception.Message, exception);

        public void Error(string message) => _router.Send(LogLevel.Error, typeof(T), message, null);
    }
}