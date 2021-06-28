using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;
using Annium.Diagnostics.Debug;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal
{
    internal class LogSentry : ILogSentry
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IList<LogMessage> _messagesBuffer = new List<LogMessage>();
        private Action<LogMessage> _handler;
        private bool _isHandlerSet;

        public LogSentry(
            ITimeProvider timeProvider
        )
        {
            _timeProvider = timeProvider;
            _handler = _messagesBuffer.Add;
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

            var msg = new LogMessage(
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

            _handler(msg);
        }

        public void SetHandler(Action<LogMessage> handler)
        {
            if (_isHandlerSet)
                throw new InvalidOperationException("Handler is intended to be set once");

            _isHandlerSet = true;
            _handler = handler;

            foreach (var message in _messagesBuffer)
                _handler(message);
        }
    }
}