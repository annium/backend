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
    internal class LogRouter : ILogRouter
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IEnumerable<ILogScheduler> _schedulers;

        public LogRouter(
            ITimeProvider timeProvider,
            IEnumerable<ILogScheduler> schedulers
        )
        {
            _timeProvider = timeProvider;
            _schedulers = schedulers;
        }

        public void Send<T>(
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

            foreach (var scheduler in _schedulers)
                if (scheduler.Filter(msg))
                    scheduler.Handle(msg);
        }
    }
}