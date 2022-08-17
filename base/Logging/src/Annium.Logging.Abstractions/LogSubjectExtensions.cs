using System.Runtime.CompilerServices;

namespace Annium.Logging.Abstractions;

public static class LogSubjectExtensions
{
    public static LogEntryContext<T> Log<T>(
        this T subject,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    ) where T : ILogSubject<T> =>
        new(subject, subject.Logger, file, member, line);
}