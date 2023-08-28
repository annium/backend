using System;
using System.Diagnostics;

namespace Annium.Logging.Abstractions;

public readonly ref struct LogEntryContext<T>
    where T : notnull
{
    private readonly T _subject;
    private readonly string _file;
    private readonly string _member;
    private readonly int _line;
    private readonly ILogger _logger;

    public LogEntryContext(
        T subject,
        ILogger logger,
        string file,
        string member,
        int line
    )
    {
        _logger = logger;
        _subject = subject;
        _file = file;
        _member = member;
        _line = line;
    }

    public void Log(LogLevel level, string message, params object[] data) =>
        _logger.Log(_subject, _file, _member, _line, level, message, data);

    [Conditional("LOG_TRACE")]
    public void Trace(string message, params object[] data) =>
        _logger.Log(_subject, _file, _member, _line, LogLevel.Trace, message, data);

    [Conditional("LOG_DEBUG")]
    public void Debug(string message, params object[] data) =>
        _logger.Log(_subject, _file, _member, _line, LogLevel.Debug, message, data);

    public void Info(string message, params object[] data) =>
        _logger.Log(_subject, _file, _member, _line, LogLevel.Info, message, data);

    public void Warn(string message, params object[] data) =>
        _logger.Log(_subject, _file, _member, _line, LogLevel.Warn, message, data);

    public void Error(Exception exception, params object[] data) =>
        _logger.Error(_subject, _file, _member, _line, exception, data);

    public void Error(string message, params object[] data) =>
        _logger.Log(_subject, _file, _member, _line, LogLevel.Error, message, data);
}