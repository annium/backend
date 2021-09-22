using System.Runtime.CompilerServices;
using System.Text;
using Annium.Core.Internal;
using Annium.Logging.Shared;

namespace Annium.Logging.Console
{
    public static class LogMessageExtensions
    {
        public static string DefaultFormat<TContext>(this LogMessage<TContext> m, string message)
            where TContext : class, ILogContext
            => DefaultFormatInternal(m, m.TimeFormat(), message);

        public static string DefaultTestFormat<TContext>(this LogMessage<TContext> m, string message)
            where TContext : class, ILogContext
            => DefaultFormatInternal(m, m.TestTimeFormat(), message);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string DefaultFormatInternal<TContext>(this LogMessage<TContext> m, string time, string message)
            where TContext : class, ILogContext
        {
            var sb = new StringBuilder();
            sb.Append(m.Subject());
            if (m.Line != 0)
                sb.Append(m.Location());

            return $"[{time}] {m.Level} [{m.ThreadId:D3}] {sb} >> {message}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Subject<TContext>(this LogMessage<TContext> m)
            where TContext : class, ILogContext
            => string.IsNullOrWhiteSpace(m.SubjectType) ? m.Source : $"{m.SubjectType}#{m.SubjectId}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Location<TContext>(this LogMessage<TContext> m)
            where TContext : class, ILogContext
            => $" at {m.Type}.{m.Member}:{m.Line}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string TimeFormat<TContext>(this LogMessage<TContext> m)
            where TContext : class, ILogContext
            => m.Instant.InZone(ConsoleLogHandler<TContext>.Tz).LocalDateTime.ToString("HH:mm:ss.fff", null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string TestTimeFormat<TContext>(this LogMessage<TContext> m)
            where TContext : class, ILogContext
            => Log.ToRelativeLogTime(m.Instant.InZone(ConsoleLogHandler<TContext>.Tz).LocalDateTime.ToDateTimeUnspecified()).ToString("hh\\:mm\\:ss\\.fff");
    }
}