using System;
using System.Threading.Tasks;
using Annium.Extensions.Execution;

namespace Annium.Logging.Shared.Internal
{
    internal class LogScheduler : IAsyncDisposable
    {
        public Func<LogMessage, bool> Filter { get; }
        private readonly ILogHandler _handler;
        private readonly IBackgroundExecutor _executor;

        public LogScheduler(
            Func<LogMessage, bool> filter,
            ILogHandler handler
        )
        {
            Filter = filter;
            _handler = handler;

            _executor = Executor.Background.Sequential<ILogHandler>();
            _executor.Start();
        }

        public void Handle(LogMessage message)
        {
            _executor.Schedule(() => _handler.Handle(message));
        }


        public async ValueTask DisposeAsync()
        {
            await _executor.DisposeAsync();
        }
    }
}