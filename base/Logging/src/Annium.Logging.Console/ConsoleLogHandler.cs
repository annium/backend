using System;
using System.Collections.Generic;
using Annium.Logging.Abstractions;
using Annium.Logging.Shared;
using NodaTime;

namespace Annium.Logging.Console
{
    internal class ConsoleLogHandler<TContext> : ILogHandler<TContext>
        where TContext : class, ILogContext
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

        private readonly Func<LogMessage<TContext>, string, string> _format;
        private readonly bool _color;

        public ConsoleLogHandler(
            Func<LogMessage<TContext>, string, string> format,
            bool color
        )
        {
            _format = format;
            _color = color;
        }

        public void Handle(LogMessage<TContext> msg)
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

        private void WriteLine(LogMessage<TContext> msg, string message) => System.Console.WriteLine(_format(msg, message));

        private string GetExceptionMessage(Exception e) => $"{e.Message}{Environment.NewLine}{e.StackTrace}";
    }
}