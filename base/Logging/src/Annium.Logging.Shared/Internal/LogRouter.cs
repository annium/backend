using System.Collections.Generic;

namespace Annium.Logging.Shared.Internal
{
    internal class LogRouter
    {
        private readonly IEnumerable<ILogScheduler> _schedulers;

        public LogRouter(
            ILogSentry sentry,
            IEnumerable<ILogScheduler> schedulers
        )
        {
            sentry.SetHandler(Send);
            _schedulers = schedulers;
        }

        private void Send(LogMessage msg)
        {
            foreach (var scheduler in _schedulers)
                if (scheduler.Filter(msg))
                    scheduler.Handle(msg);
        }
    }
}