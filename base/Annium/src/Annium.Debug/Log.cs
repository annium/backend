using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Debug.Internal;

namespace Annium.Debug;

public static class Log
{
    private const string LogVar = "ANNIUM_LOG";
    private static bool Enabled { get; }
    public static Action<string> Write { get; set; }
    private static IReadOnlyCollection<string> Filter { get; }
    private static DateTime Since { get; }
    private static Func<string> GetLogTime { get; set; }

    static Log()
    {
        Since = DateTime.Now;
        GetLogTime = GetAbsoluteLogTime;
        (Enabled, Write, Filter) = Configure();
    }

    public static void SetTestMode() => GetLogTime = GetRelativeLogTime;

    public static TimeSpan ToRelativeLogTime(DateTime dt) => dt - Since;

    [Conditional("LOG_CORE")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Trace(
        string subject,
        string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        if (!Enabled)
            return;

        var caller = Path.GetFileNameWithoutExtension(callerFilePath);
        if (Filter.Count == 0 || Filter.Any(caller.Contains))
            Write(
                $"[{GetLogTime()}] ADBG [{Thread.CurrentThread.ManagedThreadId:D3}] {subject} at {caller}.{member}:{line} >> {message}"
            );
    }

    [Conditional("LOG_CORE")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Trace(
        string message,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        if (!Enabled)
            return;

        var caller = Path.GetFileNameWithoutExtension(callerFilePath);
        if (Filter.Count == 0 || Filter.Any(caller.Contains))
            Write(
                $"[{GetLogTime()}] ADBG [{Thread.CurrentThread.ManagedThreadId:D3}] {caller}.{member}:{line} >> {message}"
            );
    }

    private static Config Configure()
    {
        var raw = Environment.GetEnvironmentVariable(LogVar);
        if (raw is null)
            return new(false, Console.WriteLine, new());

        // convert to cfg dictionary
        var cfg = raw
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split('=', StringSplitOptions.RemoveEmptyEntries))
            .Where(x => x.Length == 2)
            .ToDictionary(x => x[0], x => x[1]);

        // resolve device
        if (!cfg.TryGetValue("dev", out var rawDevice) || !Enum.TryParse<LogDevice>(rawDevice, out var device))
            device = LogDevice.Console;
        var write = ResolveWrite(device, cfg.TryGetValue("addr", out var address) ? address : string.Empty);

        // resolve filter
        var filter = cfg.TryGetValue("filter", out var rawFilter)
            ? rawFilter.Split(',').ToList()
            : new List<string>();

        return new(true, write, filter);
    }

    private static Action<string> ResolveWrite(LogDevice dev, string address)
    {
        switch (dev)
        {
            case LogDevice.Console:
                return Console.WriteLine;
            case LogDevice.Tcp:
                if (string.IsNullOrWhiteSpace(address))
                    throw new ArgumentNullException(
                        $"To use {dev}, specify {nameof(IPEndPoint)} in {nameof(address)}  variable");
                Console.WriteLine($"Send logs to {address}");
                return new TcpLogger(address).Write;
            default:
                throw new NotSupportedException($"Device {dev} support is not implemented yet");
        }
    }

    private record Config(bool Enabled, Action<string> Write, List<string> Filter);

    private static string GetAbsoluteLogTime() => DateTime.Now.ToString("HH:mm:ss.fff");
    private static string GetRelativeLogTime() => (DateTime.Now - Since).ToString(@"hh\:mm\:ss\.fff");
}