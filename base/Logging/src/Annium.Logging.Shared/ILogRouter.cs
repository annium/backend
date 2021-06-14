using System;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared
{
    public interface ILogRouter
    {
        void Send(
            ILogSubject? subject,
            LogLevel level,
            string source,
            string message,
            Exception? exception,
            object[] data
        );
    }
}