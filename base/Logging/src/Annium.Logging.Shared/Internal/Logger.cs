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

        public void Log<TS>(TS? subject, string file, string member, int line, LogLevel level, string message, object[] data) where TS : class, ILogSubject =>
            _router.Send(subject, file, member, line, level, typeof(T).FriendlyName(), message, null, data);

        public void Error<TS>(TS? subject, string file, string member, int line, Exception exception, object[] data) where TS : class, ILogSubject =>
            _router.Send(subject, file, member, line, LogLevel.Error, typeof(T).FriendlyName(), exception.Message, exception, data);
    }
}