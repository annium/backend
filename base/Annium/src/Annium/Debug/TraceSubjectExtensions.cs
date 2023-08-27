using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Annium.Debug;

public static class TraceSubjectExtensions
{
    [Conditional("LOG_CORE")]
    public static void Trace<T>(
        this T subject,
        string message = "",
        bool withTrace = false,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    ) where T : ITraceSubject =>
        subject.Tracer.Trace(subject, message, withTrace, file, member, line);
}