using System;

namespace Annium.Logging.Abstractions
{
    public interface ILogRouter
    {
        void Send(LogLevel level, Type source, string message, Exception? exception);
    }
}