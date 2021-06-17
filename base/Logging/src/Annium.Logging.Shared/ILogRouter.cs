using System;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared
{
    public interface ILogRouter
    {
        void Send<T>(
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
    }
}