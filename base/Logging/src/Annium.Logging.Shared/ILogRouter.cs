using System;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared
{
    public interface ILogRouter
    {
        void Send(LogLevel level, Type source, string message, Exception? exception);
    }
}