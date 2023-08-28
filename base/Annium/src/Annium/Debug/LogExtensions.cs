using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Annium.Debug;

public static class LogExtensions
{
    // [Conditional("LOG_CORE")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Trace<T>(
        this T obj,
        string message = "",
        bool withTrace = false,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
        where T : class
    {
        var subject = $"{obj.GetType().FriendlyName()}#{obj.GetId()}";
        var caller = Path.GetFileNameWithoutExtension(callerFilePath);
        var trace = withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ADBG [{Environment.CurrentManagedThreadId:D3}] {subject} at {caller}.{member}:{line} >> {message}{trace}");
    }
}