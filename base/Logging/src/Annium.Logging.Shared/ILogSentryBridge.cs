using System;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared;

public interface ILogSentryBridge
{
    public void Register<T>(
        T? subject,
        string file,
        string member,
        int line,
        LogLevel level,
        string source,
        string messageTemplate,
        Exception? exception,
        object[] dataItems
    );
}