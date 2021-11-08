using System;

namespace Annium.Logging.Shared
{
    public static class LogMessageProcessor
    {
        public static void Process<TContext>(LogMessage<TContext> msg, Action<LogMessage<TContext>, string> addEvent)
            where TContext : class, ILogContext
        {
            if (msg.Exception is AggregateException aggregateException)
            {
                var errors = aggregateException.Flatten().InnerExceptions;
                addEvent(msg, $"{errors.Count} error(s) in: {GetExceptionMessage(aggregateException)}");

                foreach (var error in errors)
                    addEvent(msg, GetExceptionMessage(error));
            }
            else if (msg.Exception != null)
                addEvent(msg, GetExceptionMessage(msg.Exception));
            else
                addEvent(msg, msg.Message);
        }

        private static string GetExceptionMessage(Exception e) => $"{e.Message}{e.StackTrace}";
    }
}