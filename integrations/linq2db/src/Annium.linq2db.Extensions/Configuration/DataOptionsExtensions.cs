using System;
using System.Diagnostics;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using LinqToDB;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public static class DataOptionsExtensions
{
    public static DataOptions UseLogging(this DataOptions options, IServiceProvider sp)
    {
        var logSubject = sp.Resolve<ILogSubject>();

        return options
            .UseTraceLevel(TraceLevel.Verbose)
            .UseTraceWith((msg, category, lvl) =>
            {
                switch (lvl)
                {
                    case TraceLevel.Error:
                        logSubject.Error<string?, string?>("{category}: {msg}", category, msg);
                        break;
                    case TraceLevel.Warning:
                        logSubject.Warn<string?, string?>("{category}: {msg}", category, msg);
                        break;
                    // all other levels are handled as Trace by design (at the moment linq2db traces with Info level)
                    default:
                        logSubject.Trace<string?, string?>("{category}: {msg}", category, msg);
                        break;
                }
            });
    }
}