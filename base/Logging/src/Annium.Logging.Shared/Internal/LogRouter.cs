using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;
using Annium.Diagnostics.Debug;
using Annium.Extensions.Execution;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal
{
    internal class LogRouter : ILogRouter
    {
        private readonly ITimeProvider _timeProvider;
        private readonly Dictionary<LogRoute, (IBackgroundExecutor, ILogHandler)> _handlers;

        public LogRouter(
            ITimeProvider timeProvider,
            IEnumerable<LogRoute> routes,
            IServiceProvider provider
        )
        {
            _timeProvider = timeProvider;
            _handlers = routes.ToDictionary(
                x => x,
                x =>
                {
                    var executor = Executor.Background.Sequential<ILogHandler>();
                    executor.Start();

                    return (
                        executor,
                        (ILogHandler) provider.Resolve(x.Service!.ServiceType)
                    );
                }
            );
        }

        public void Send<T>(
            T? subject,
            string file,
            string member,
            int line,
            LogLevel level,
            string source,
            string message,
            Exception? exception,
            object[] data
        )
            where T : class, ILogSubject
        {
            var instant = _timeProvider.Now;

            var msg = new LogMessage(
                instant,
                subject?.GetType().FriendlyName() ?? null,
                subject?.GetId() ?? null,
                level,
                source,
                Thread.CurrentThread.ManagedThreadId,
                message,
                exception?.Demystify(),
                data,
                Path.GetFileNameWithoutExtension(file),
                member,
                line
            );

            foreach (var (route, (executor, handler)) in _handlers)
                if (route.Filter(msg))
                    executor.Schedule(() => handler.Handle(msg));
        }
    }
}