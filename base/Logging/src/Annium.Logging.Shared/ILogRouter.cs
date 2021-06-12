using System;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared
{
    public interface ILogRouter
    {
        void Send(
            LogLevel level,
            string source,
            string message,
            Exception? exception,
            object? subject,
            object? data,
            bool withTrace,
            string file,
            string member,
            int line
        );
    }
}