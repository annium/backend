using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Annium.Core.Internal
{
    public static class Framework
    {
        private static SetupMode Mode { get; }
        private const string ModeVar = "ANNIUM_MODE";

        static Framework()
        {
            var modeValue = Environment.GetEnvironmentVariable(ModeVar);
            if (modeValue is not null && Enum.TryParse(modeValue, true, out SetupMode mode))
                Mode = mode;
        }

        public static void Log(
            Func<string> getMessage,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Mode == SetupMode.Debug)
            {
                var caller = Path.GetFileNameWithoutExtension(callerFilePath);
                Console.WriteLine($"{caller}.{member}: {getMessage()}");
            }
        }

        public static void Log(
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string member = ""
        )
        {
            if (Mode == SetupMode.Debug)
            {
                var caller = Path.GetFileNameWithoutExtension(callerFilePath);
                Console.WriteLine($"{caller}.{member}");
            }
        }
    }
}