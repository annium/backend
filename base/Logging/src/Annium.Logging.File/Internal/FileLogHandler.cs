using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Logging.Shared;

namespace Annium.Logging.File.Internal
{
    internal class FileLogHandler<TContext> : BufferingLogHandler<TContext, string>
        where TContext : class, ILogContext
    {
        private readonly FileLoggingConfiguration _cfg;

        public FileLogHandler(
            Func<LogMessage<TContext>, string, string> format,
            FileLoggingConfiguration cfg
        ) : base(format, cfg)
        {
            _cfg = cfg;
        }

        protected override async ValueTask<bool> SendEventsAsync(IReadOnlyCollection<string> events)
        {
            try
            {
                await System.IO.File.AppendAllLinesAsync(_cfg.File, events);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to to write events to {_cfg.File}: {e}");
                return false;
            }
        }
    }
}