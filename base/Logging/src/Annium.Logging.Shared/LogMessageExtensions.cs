using System.Runtime.CompilerServices;
using System.Text;
using Annium.Debug;
using NodaTime;

namespace Annium.Logging.Shared;

public static class LogMessageExtensions
{
    public static readonly DateTimeZone Tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();

    public static string DefaultFormat<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => DefaultFormat(m, m.TimeFormat());

    public static string DefaultTestFormat<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => DefaultFormat(m, m.TestTimeFormat());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string DefaultFormat<TContext>(this LogMessage<TContext> m, string time)
        where TContext : class, ILogContext
    {
        var sb = new StringBuilder();
        sb.Append(m.Subject());
        if (m.Line != 0)
            sb.Append($" at {m.Location()}");

        return $"[{time}] {m.Level} [{m.ThreadId:D3}] {sb} >> {m.Message}";
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
    public static string DateTimeFormat<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => m.Instant.InZone(Tz).LocalDateTime.ToString("dd.MM.yy HH:mm:ss.fff", null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string TimeFormat<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => m.Instant.InZone(Tz).LocalDateTime.ToString("HH:mm:ss.fff", null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string TestDateTimeFormat<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => Log.ToRelativeLogTime(m.Instant.InZone(Tz).LocalDateTime.ToDateTimeUnspecified()).ToString("ddd\\.hh\\:mm\\:ss\\.fff");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string TestTimeFormat<TContext>(this LogMessage<TContext> m)
        where TContext : class, ILogContext
        => Log.ToRelativeLogTime(m.Instant.InZone(Tz).LocalDateTime.ToDateTimeUnspecified()).ToString("hh\\:mm\\:ss\\.fff");
}