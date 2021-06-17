using System.Runtime.CompilerServices;

namespace Annium.Logging.Abstractions
{
    public static class LogSubjectExtensions
    {
        public static LogContext<T> Log<T>(
            this T subject,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) where T : class, ILogSubject =>
            new(subject, file, member, line);
    }
}