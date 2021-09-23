using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Diagnostics.Debug;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal
{
    internal class LogSentryBridge<TContext> : ILogSentryBridge
        where TContext : class, ILogContext
    {
        private readonly ITimeProvider _timeProvider;
        private readonly TContext _context;
        private readonly ILogSentry<TContext> _logSentry;

        public LogSentryBridge(
            ITimeProvider timeProvider,
            TContext context,
            ILogSentry<TContext> logSentry
        )
        {
            _timeProvider = timeProvider;
            _context = context;
            _logSentry = logSentry;
        }

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
        )
            where T : class, ILogSubject
        {
            var instant = _timeProvider.Now;
            var (message, data) = Helper.Process(messageTemplate, dataItems);

            var msg = new LogMessage<TContext>(
                _context,
                instant,
                subject?.GetType().FriendlyName() ?? null,
                subject?.GetId() ?? null,
                level,
                source,
                Thread.CurrentThread.ManagedThreadId,
                message,
                exception?.Demystify(),
                messageTemplate,
                data,
                Path.GetFileNameWithoutExtension(file),
                member,
                line
            );

            _logSentry.Register(msg);
        }
    }
}