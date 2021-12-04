using System;
using System.Runtime.CompilerServices;
using Annium.Core.Primitives;
using Annium.Diagnostics.Debug;

namespace Annium.Core.Internal;

public static class LogExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Trace<T>(
        this T obj,
        string message = "",
        bool withTrace = false,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
        where T : class
    {
        var subject = obj.GetFullId();
        var trace = withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty;
        var msg = string.IsNullOrWhiteSpace(message) ? string.Empty : $" >> {message}";
        Log.Trace($"{subject}{trace}{msg}", callerFilePath, member, line);
    }
}