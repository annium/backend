using System.Runtime.CompilerServices;

namespace Annium.Logging.Abstractions
{
    public static class LogSubjectExtensions
    {
        public static LogContext Log(
            this ILogSubject subject,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) =>
            new(subject, file, member, line);
    }
}