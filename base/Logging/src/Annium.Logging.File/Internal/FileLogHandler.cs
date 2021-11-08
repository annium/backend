using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared;

namespace Annium.Logging.File.Internal
{
    internal class FileLogHandler<TContext> : BufferingLogHandler<TContext, string>
        where TContext : class, ILogContext
    {
        private readonly AutoResetEvent _gate = new(true);
        private readonly FileLoggingConfiguration _cfg;

        public FileLogHandler(
            Func<LogMessage<TContext>, string, string> format,
            FileLoggingConfiguration cfg
        ) : base(format, cfg)
        {
            _cfg = cfg;
            System.IO.File.WriteAllText(_cfg.File, string.Empty);
        }

        protected override async ValueTask<bool> SendEventsAsync(IReadOnlyCollection<string> events)
        {
            try
            {
                _gate.WaitOne();
                await System.IO.File.AppendAllLinesAsync(_cfg.File, events);
                await System.IO.File.AppendAllTextAsync(_cfg.File, string.Empty);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to to write events to {_cfg.File}: {e}");
                return false;
            }
            finally
            {
                _gate.Set();
            }
        }
    }
}