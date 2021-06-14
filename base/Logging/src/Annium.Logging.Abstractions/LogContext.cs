using System;

namespace Annium.Logging.Abstractions
{
    public readonly ref struct LogContext
    {
        public ILogSubject Subject { get; }
        public string File { get; }
        public string Member { get; }
        public int Line { get; }

        public LogContext(
            ILogSubject subject,
            string file,
            string member,
            int line
        )
        {
            Subject = subject;
            File = file;
            Member = member;
            Line = line;
        }

        public void Log(LogLevel level, string message, params object[] data) =>
            Subject.Logger.Log(Subject, File, Member, Line, level, message, data);

        public void Trace(string message, params object[] data) =>
            Subject.Logger.Log(Subject, File, Member, Line, LogLevel.Trace, message, data);

        public void Debug(string message, params object[] data) =>
            Subject.Logger.Log(Subject, File, Member, Line, LogLevel.Debug, message, data);

        public void Info(string message, params object[] data) =>
            Subject.Logger.Log(Subject, File, Member, Line, LogLevel.Info, message, data);

        public void Warn(string message, params object[] data) =>
            Subject.Logger.Log(Subject, File, Member, Line, LogLevel.Warn, message, data);

        public void Error(Exception exception, params object[] data) =>
            Subject.Logger.Error(Subject, File, Member, Line, exception, data);

        public void Error(string message, params object[] data) =>
            Subject.Logger.Log(Subject, File, Member, Line, LogLevel.Error, message, data);
    }
}