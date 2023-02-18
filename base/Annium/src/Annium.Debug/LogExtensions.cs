using System;
using System.Runtime.CompilerServices;
using Annium.Debug.Internal;

namespace Annium.Debug;

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
        var subject = obj is null ? "null" : $"{obj.GetType().FriendlyName()}#{obj.GetId()}";
        var trace = withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty;
        Log.Trace(subject, $"{message}{trace}", callerFilePath, member, line);
    }
}