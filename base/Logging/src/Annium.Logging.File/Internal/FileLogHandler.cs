using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared;

namespace Annium.Logging.File.Internal;

internal class FileLogHandler<TContext> : BufferingLogHandler<TContext>
    where TContext : class, ILogContext
{
    private readonly AutoResetEvent _gate = new(true);
    private readonly Func<LogMessage<TContext>, string> _format;
    private readonly FileLoggingConfiguration _cfg;

    public FileLogHandler(
        Func<LogMessage<TContext>, string> format,
        FileLoggingConfiguration cfg
    ) : base(cfg)
    {
        _format = format;
        _cfg = cfg;
        System.IO.File.WriteAllText(_cfg.File, string.Empty);
    }

    protected override async ValueTask<bool> SendEventsAsync(IReadOnlyCollection<LogMessage<TContext>> events)
    {
        try
        {
            _gate.WaitOne();
            await System.IO.File.AppendAllLinesAsync(_cfg.File, events.Select(_format));
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