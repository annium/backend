using System;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal;

internal class Logger<T> : ILogger<T>
{
    private readonly ILogSentryBridge _sentryBridge;

    public Logger(
        ILogSentryBridge sentryBridge
    )
    {
        _sentryBridge = sentryBridge;
    }

    public void Log<TS>(TS? subject, string file, string member, int line, LogLevel level, string message, object[] data) =>
        _sentryBridge.Register(subject, file, member, line, level, typeof(T).FriendlyName(), message, null, data);

    public void Error<TS>(TS? subject, string file, string member, int line, Exception exception, object[] data) =>
        _sentryBridge.Register(subject, file, member, line, LogLevel.Error, typeof(T).FriendlyName(), exception.Message, exception, data);
}