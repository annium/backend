using System;
using Annium.Logging.Shared;

namespace Annium.Logging.Console.Internal;

internal class ConsoleLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class, ILogContext
{
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
        lock (StaticState.ConsoleLock)
        {
            var currentColor = _color ? System.Console.ForegroundColor : default;
            try
            {
                if (_color)
                    System.Console.ForegroundColor = StaticState.LevelColors[msg.Level];

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