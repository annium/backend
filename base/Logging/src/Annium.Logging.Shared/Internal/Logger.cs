using System;

namespace Annium.Logging.Shared.Internal;

internal class Logger : ILogger
{
    private readonly ILogSentryBridge _sentryBridge;

    public Logger(
        ILogSentryBridge sentryBridge
    )
    {
        _sentryBridge = sentryBridge;
    }

    public void Log<T>(T subject, string file, string member, int line, LogLevel level, string message, object[] data)
        where T : notnull
    {
        _sentryBridge.Register(subject.GetType().FriendlyName(), subject.GetFullId(), file, member, line, level, message, null, data);
    }

    public void Error<T>(T subject, string file, string member, int line, Exception exception, object[] data)
        where T : notnull
    {
        _sentryBridge.Register(subject.GetType().FriendlyName(), subject.GetFullId(), file, member, line, LogLevel.Error, exception.Message, exception, data);
    }
}