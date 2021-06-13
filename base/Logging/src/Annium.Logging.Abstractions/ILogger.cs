using System;
using System.Runtime.CompilerServices;

namespace Annium.Logging.Abstractions
{
    public interface ILogger
    {
        void Log(
            LogLevel level,
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        );

        void Trace(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        );

        void Debug(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        );

        void Info(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        );

        void Warn(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        );

        void Error(
            Exception exception,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        );

        void Error(
            string message,
            object? subject = default!,
            object? data = default!,
            bool withTrace = false,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        );
    }

    public interface ILogger<out T> : ILogger
    {
    }
}