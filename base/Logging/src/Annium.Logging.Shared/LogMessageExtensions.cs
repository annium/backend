using System.Runtime.CompilerServices;
using System.Text;
using NodaTime;

namespace Annium.Logging.Shared;

public static class LogMessageExtensions
{
    public static readonly DateTimeZone Tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();

    public static string DefaultFormat<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
    {
        var sb = new StringBuilder();
        sb.Append(m.Subject());
        if (m.Line != 0)
            sb.Append($" at {m.Location()}");

        return $"[{m.Time()}] {m.Level} [{m.ThreadId:D3}] {sb} >> {m.Message}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Subject<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => string.IsNullOrWhiteSpace(m.SubjectType) ? m.Source : $"{m.SubjectType}#{m.SubjectId}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Location<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => $"{m.Type}.{m.Member}:{m.Line}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string DateTime<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => m.Instant.InZone(Tz).LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Time<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => m.Instant.InZone(Tz).LocalDateTime.ToString("HH:mm:ss.fff", null);
}