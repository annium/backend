using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Annium.Core.Internal
{
    public static class Log
    {
        private static LogLevel Mode { get; } = LogLevel.Release;
        private const string ModeVar = "ANNIUM_MODE";

        static Log()
        {
            var modeValue = Environment.GetEnvironmentVariable(ModeVar);
            if (modeValue is not null && Enum.TryParse(modeValue, true, out LogLevel mode))
                Mode = mode;
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
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ADBG:{Mode} {caller}.{member}{message}");
        }
    }
}