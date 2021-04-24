using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Annium.Core.Primitives;

namespace Annium.Core.Internal
{
    public static class Log
    {
        private const string LevelVar = "ANNIUM_LOG_LEVEL";
        private const string DeviceVar = "ANNIUM_LOG_DEVICE";
        private const string DeviceAddressVar = "ANNIUM_LOG_DEVICE_ADDRESS";
        private const string FilterVar = "ANNIUM_LOG_FILTER";
        private static LogLevel Level { get; } = LogLevel.Release;
        private static Action<string> Write { get; set; } = Console.WriteLine;
        private static List<string> Filter { get; } = new();

        static Log()
        {
            var levelValue = Environment.GetEnvironmentVariable(LevelVar);
            if (levelValue is not null && levelValue.TryParseEnum<LogLevel>(out var level))
                Level = level;

            var deviceValue = Environment.GetEnvironmentVariable(DeviceVar);
            if (deviceValue is not null && deviceValue.TryParseEnum<LogDevice>(out var device))
                Write = ResolveWrite(device);

            var filterValue = Environment.GetEnvironmentVariable(FilterVar);
            if (filterValue is not null)
                Filter = filterValue.Split(',').ToList();

            Action<string> ResolveWrite(LogDevice dev)
            {
                switch (dev)
                {
                    case LogDevice.Console:
                        return Console.WriteLine;
                    case LogDevice.Tcp:
                        var address = Environment.GetEnvironmentVariable(DeviceAddressVar) ??
                                      throw new ArgumentNullException(
                                          $"To use {dev}, specify {nameof(IPEndPoint)} in {DeviceAddressVar} env variable");
                        var endpoint = IPEndPointExt.Parse(address);
                        var client = new TcpClient();
                        client.Connect(endpoint);
                        var ns = client.GetStream();
                        return entry =>
                        {
                            var msg = Encoding.UTF8.GetBytes($"{entry}{Environment.NewLine}");
                            ns.Write(msg);
                        };
                    default:
                        throw new NotSupportedException($"Device {dev} support is not implemented yet");
                }
            }
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
    }
}