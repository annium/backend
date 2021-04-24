using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Core.Internal
{
    public static class Log
    {
        private const string ModeVar = "ANNIUM_MODE";
        private const string LogVar = "ANNIUM_Log";
        private static LogLevel Mode { get; } = LogLevel.Release;
        private static List<string> LogFilters { get; } = new();

        static Log()
        {
            var modeValue = Environment.GetEnvironmentVariable(ModeVar);
            if (modeValue is not null && Enum.TryParse(modeValue, true, out LogLevel mode))
                Mode = mode;

            var logValue = Environment.GetEnvironmentVariable(LogVar);
            if (logValue is not null)
                LogFilters = logValue.Split(',').ToList();
        }

        public static void Debug(
            Func<string> getMessage,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Mode >= LogLevel.Debug)
                LogInternal(callerFilePath, member, $": {getMessage()}");
        }

        public static void Debug(
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Mode >= LogLevel.Debug)
                LogInternal(callerFilePath, member);
        }

        public static void Trace(
            Func<string> getMessage,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Mode >= LogLevel.Trace)
                LogInternal(callerFilePath, member, $": {getMessage()}");
        }

        public static void Trace(
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Mode >= LogLevel.Trace)
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
            if (LogFilters.Count == 0 || LogFilters.Any(caller.Contains))
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ADBG:{Mode} {caller}.{member}{message}");
        }
    }
}