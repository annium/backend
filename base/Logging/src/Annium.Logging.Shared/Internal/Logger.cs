using System;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal
{
    internal class Logger<T> : ILogger<T>
    {
        private readonly ILogRouter _router;

        public Logger(
            ILogRouter router
        )
        {
            _router = router;
        }

        public void Log(ILogSubject subject, LogLevel level, string message, object[] data) =>
            _router.Send(subject, level, typeof(T).FriendlyName(), message, null, data);

        public void Trace(ILogSubject subject, string message, object[] data) =>
            _router.Send(subject, LogLevel.Trace, typeof(T).FriendlyName(), message, null, data);

        public void Debug(ILogSubject subject, string message, object[] data) =>
            _router.Send(subject, LogLevel.Debug, typeof(T).FriendlyName(), message, null, data);

        public void Info(ILogSubject subject, string message, object[] data) =>
            _router.Send(subject, LogLevel.Info, typeof(T).FriendlyName(), message, null, data);

        public void Warn(ILogSubject subject, string message, object[] data) =>
            _router.Send(subject, LogLevel.Warn, typeof(T).FriendlyName(), message, null, data);

        public void Error(ILogSubject subject, Exception exception, object[] data) =>
            _router.Send(subject, LogLevel.Error, typeof(T).FriendlyName(), exception.Message, exception, data);

        public void Error(ILogSubject subject, string message, object[] data) =>
            _router.Send(subject, LogLevel.Error, typeof(T).FriendlyName(), message, null, data);
    }
}