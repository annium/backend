using System;
using System.Runtime.CompilerServices;

namespace Annium.Logging;

public static class LogSubjectWarnExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, Array.Empty<object>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        object x1,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new[] { x1 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        object x1,
        object x2,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new[] { x1, x2 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        object x1,
        object x2,
        object x3,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new[] { x1, x2, x3 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        object x1,
        object x2,
        object x3,
        object x4,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new[] { x1, x2, x3, x4 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        object x1,
        object x2,
        object x3,
        object x4,
        object x5,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new[] { x1, x2, x3, x4, x5 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        object x1,
        object x2,
        object x3,
        object x4,
        object x5,
        object x6,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new[] { x1, x2, x3, x4, x5, x6 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        object x1,
        object x2,
        object x3,
        object x4,
        object x5,
        object x6,
        object x7,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new[] { x1, x2, x3, x4, x5, x6, x7 });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warn(
        this ILogSubject subject,
        string message,
        object x1,
        object x2,
        object x3,
        object x4,
        object x5,
        object x6,
        object x7,
        object x8,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        subject.Logger.Log(subject, file, member, line, LogLevel.Warn, message, new[] { x1, x2, x3, x4, x5, x6, x7, x8 });
    }
}