using System;
using System.Threading.Tasks;
using Annium.Extensions.Execution;

namespace Annium.Logging.Shared.Internal
{
    internal class BackgroundLogScheduler : ILogScheduler, IAsyncDisposable
    {
        public Func<LogMessage, bool> Filter { get; }
        private readonly IAsyncLogHandler _handler;
        private readonly LogRouteConfiguration _configuration;
        private readonly IBackgroundExecutor _executor;

        public BackgroundLogScheduler(
            Func<LogMessage, bool> filter,
            IAsyncLogHandler handler,
            LogRouteConfiguration configuration
        )
        {
            Filter = filter;
            _handler = handler;
            _configuration = configuration;

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