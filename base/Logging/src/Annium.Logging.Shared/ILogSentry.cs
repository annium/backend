using System;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared
{
    public interface ILogSentry
    {
        void Register<T>(
            T? subject,
            string file,
            string member,
            int line,
            LogLevel level,
            string source,
            string message,
            Exception? exception,
            object[] data
        ) where T : class, ILogSubject;

        void SetHandler(Action<LogMessage> handler);
    }
}