using System;
using System.Collections.Generic;
using System.Text;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Logging.Console
{
    internal class ConsoleLogHandler : ILogHandler
    {
        private static readonly DateTimeZone tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();
        private static readonly object consoleLock = new object();
        private static readonly IReadOnlyDictionary<LogLevel, ConsoleColor> levelColors;

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
            levelColors = colors;
        }

        public void Handle(LogMessage msg)
        {
            lock (consoleLock)
            {
                var currentColor = System.Console.ForegroundColor;
                try
                {
                    System.Console.ForegroundColor = levelColors[msg.Level];

                    if (msg.Exception is AggregateException aggregateException)
                    {
                        var errors = aggregateException.Flatten().InnerExceptions;
                        WriteLine(msg.Instant, $"Errors ({errors.Count}):");

                        foreach (var error in errors)
                            WriteLine(msg.Instant, GetExceptionMessage(error));
                    }
                    else if (msg.Exception is Exception)
                        WriteLine(msg.Instant, GetExceptionMessage(msg.Exception));
                    else
                        WriteLine(msg.Instant, msg.Message);
                }
                finally
                {
                    System.Console.ForegroundColor = currentColor;
                }
            }
        }

        private void WriteLine(Instant instant, string message)
        {
            var builder = new StringBuilder();

            builder.Append($"[{instant.InZone(tz).LocalDateTime.ToString("HH:mm:ss.fff", null)}] ");

            builder.Append(message);

            System.Console.WriteLine(builder.ToString());
        }

        private string GetExceptionMessage(Exception e) => $"{e.Message}{Environment.NewLine}{e.StackTrace}";
    }
}