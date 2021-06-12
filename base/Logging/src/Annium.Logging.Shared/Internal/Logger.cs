using System;
using System.Runtime.CompilerServices;
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

        public void Log(
            LogLevel level,
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) => _router.Send(level, typeof(T).FriendlyName(), message, null, subject, data, withTrace, file, member, line);

        public void Trace(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) => _router.Send(LogLevel.Trace, typeof(T).FriendlyName(), message, null, subject, data, withTrace, file, member, line);

        public void Debug(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) => _router.Send(LogLevel.Debug, typeof(T).FriendlyName(), message, null, subject, data, withTrace, file, member, line);

        public void Info(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) => _router.Send(LogLevel.Info, typeof(T).FriendlyName(), message, null, subject, data, withTrace, file, member, line);

        public void Warn(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) => _router.Send(LogLevel.Warn, typeof(T).FriendlyName(), message, null, subject, data, withTrace, file, member, line);

        public void Error(
            Exception exception,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) => _router.Send(LogLevel.Error, typeof(T).FriendlyName(), exception.Message, exception, subject, data, withTrace, file, member, line);

        public void Error(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) => _router.Send(LogLevel.Error, typeof(T).FriendlyName(), message, null, subject, data, withTrace, file, member, line);
    }
}