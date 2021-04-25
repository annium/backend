using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Annium.Core.Internal.Internal;
using Annium.Core.Primitives;

namespace Annium.Core.Internal
{
    public static class Log
    {
        private const string LogVar = "ANNIUM_LOG";
        private static LogLevel Level { get; }
        private static Action<string> Write { get; set; }
        private static List<string> Filter { get; }

        static Log()
        {
            (Level, Write, Filter) = Configure();
        }

        public static void Debug(
            Func<string> getMessage,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Level >= LogLevel.Debug)
                LogInternal(callerFilePath, member, $": {getMessage()}");
        }

        public static void Debug(
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Level >= LogLevel.Debug)
                LogInternal(callerFilePath, member);
        }

        public static void Trace(
            Func<string> getMessage,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Level >= LogLevel.Trace)
                LogInternal(callerFilePath, member, $": {getMessage()}");
        }

        public static void Trace(
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Level >= LogLevel.Trace)
                LogInternal(callerFilePath, member);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogInternal(
            string callerFilePath,
            string member,
            string message = ""
        )
        {
            var caller = Path.GetFileNameWithoutExtension(callerFilePath);
            if (Filter.Count == 0 || Filter.Any(caller.Contains))
                Write($"[{DateTime.Now:HH:mm:ss.fff}] ADBG:{Level} {caller}.{member}{message}");
        }

        private static Config Configure()
        {
            var raw = Environment.GetEnvironmentVariable(LogVar);
            if (string.IsNullOrWhiteSpace(raw))
                return new(LogLevel.Release, Console.WriteLine, new());

            // convert to cfg dictionary
            var cfg = raw.Split(';').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);

            // resolve level
            if (!cfg.TryGetValue("lvl", out var rawLevel) || !rawLevel.TryParseEnum<LogLevel>(out var level))
                level = LogLevel.Release;

            // resolve device
            if (!cfg.TryGetValue("dev", out var rawDevice) || !rawDevice.TryParseEnum<LogDevice>(out var device))
                device = LogDevice.Console;
            var write = ResolveWrite(device, cfg.TryGetValue("addr", out var address) ? address : string.Empty);

            // resolve filter
            var filter = cfg.TryGetValue("filter", out var rawFilter)
                ? rawFilter.Split(',').ToList()
                : new List<string>();

            return new(level, write, filter);
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

        private record Config(LogLevel Level, Action<string> Write, List<string> Filter);
    }
}