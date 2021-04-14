using System;
using System.Collections.Generic;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Logging.Console
{
    internal class ConsoleLogHandler : ILogHandler
    {
        public static readonly DateTimeZone Tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
        private static readonly object ConsoleLock = new();
        private static readonly IReadOnlyDictionary<LogLevel, ConsoleColor> LevelColors;

        static ConsoleLogHandler()
        {
            var colors = new Dictionary<LogLevel, ConsoleColor>
            {
                [LogLevel.Trace] = ConsoleColor.DarkGray,
                [LogLevel.Debug] = ConsoleColor.Gray,
                [LogLevel.Info] = ConsoleColor.White,
                [LogLevel.Warn] = ConsoleColor.Yellow,
                [LogLevel.Error] = ConsoleColor.Red
            };
            LevelColors = colors;
        }

        private readonly Func<LogMessage, string, string> _format;
        private readonly bool _color;

        public ConsoleLogHandler(
            Func<LogMessage, string, string> format,
            bool color
        )
        {
            _format = format;
            _color = color;
        }

        public void Handle(LogMessage msg)
        {
            lock (ConsoleLock)
            {
                var currentColor = _color ? System.Console.ForegroundColor : default;
                try
                {
                    if (_color)
                        System.Console.ForegroundColor = LevelColors[msg.Level];

                    if (msg.Exception is AggregateException aggregateException)
                    {
                        var errors = aggregateException.Flatten().InnerExceptions;
                        WriteLine(msg, $"Errors ({errors.Count}):");

                        foreach (var error in errors)
                            WriteLine(msg, GetExceptionMessage(error));
                    }
                    else if (msg.Exception != null)
                        WriteLine(msg, GetExceptionMessage(msg.Exception));
                    else
                        WriteLine(msg, msg.Message);
                }
                finally
                {
                    if (_color)
                        System.Console.ForegroundColor = currentColor;
                }
            }
        }

        private void WriteLine(LogMessage msg, string message) => System.Console.WriteLine(_format(msg, message));

        private string GetExceptionMessage(Exception e) => $"{e.Message}{Environment.NewLine}{e.StackTrace}";
    }
}