using System;
using System.Collections.Generic;
using System.Text;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Logging.Console
{
    internal class ConsoleLogHandler : ILogHandler
    {
        private static readonly DateTimeZone Tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
        private static readonly object ConsoleLock = new object();
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

        private readonly bool _time;
        private readonly bool _level;
        private readonly bool _color;

        public ConsoleLogHandler(
            bool time,
            bool level,
            bool color
        )
        {
            _time = time;
            _level = level;
            _color = color;
        }

        public void Handle(LogMessage msg)
        {
            lock (ConsoleLock)
            {
                var currentColor = System.Console.ForegroundColor;
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

        private void WriteLine(LogMessage msg, string message)
        {
            var builder = new StringBuilder();

            if (_time)
                builder.Append($"[{msg.Instant.InZone(Tz).LocalDateTime.ToString("HH:mm:ss.fff", null)}] ");

            if (_level)
                builder.Append($"{msg.Level,5}: ");

            builder.Append(message);

            System.Console.WriteLine(builder.ToString());
        }

        private string GetExceptionMessage(Exception e) => $"{e.Message}{Environment.NewLine}{e.StackTrace}";
    }
}