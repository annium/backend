using System;
using System.IO;

namespace Annium.Debug;

public class ConsoleTracer : ITracer
{
    // TODO: perhaps, drop, cause all injection will be through DI
    public static readonly ITracer Instance = new ConsoleTracer();

    private ConsoleTracer()
    {
    }

    public void Trace<T>(
        T subject,
        string message,
        bool withTrace,
        string file,
        string member,
        int line
    )
        where T : notnull
    {
        var subjectString = $"{subject.GetType().FriendlyName()}#{subject.GetId()}";
        var caller = Path.GetFileNameWithoutExtension(file);
        var trace = withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ADBG [{Environment.CurrentManagedThreadId:D3}] {subjectString} at {caller}.{member}:{line} >> {message}{trace}");
    }
}