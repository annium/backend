using System;

namespace Annium.Logging.Abstractions
{
    public static class LogSubjectExtensions
    {
        public static void Log(this ILogSubject subject, LogLevel level, string message, params object[] data) =>
            subject.Logger.Log(subject, level, message, data);

        public static void Trace(this ILogSubject subject, string message, params object[] data) =>
            subject.Logger.Trace(subject, message, data);

        public static void Debug(this ILogSubject subject, string message, params object[] data) =>
            subject.Logger.Debug(subject, message, data);

        public static void Info(this ILogSubject subject, string message, params object[] data) =>
            subject.Logger.Info(subject, message, data);

        public static void Warn(this ILogSubject subject, string message, params object[] data) =>
            subject.Logger.Warn(subject, message, data);

        public static void Error(this ILogSubject subject, Exception exception, params object[] data) =>
            subject.Logger.Error(subject, exception, data);

        public static void Error(this ILogSubject subject, string message, params object[] data) =>
            subject.Logger.Error(subject, message, data);
    }
}