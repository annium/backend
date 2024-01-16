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
        var logBridgeFactory = sp.Resolve<ILogBridgeFactory>();

        return options
            .UseTraceLevel(TraceLevel.Verbose)
            .UseTraceWith(
                (msg, category, lvl) =>
                {
                    var logger = logBridgeFactory.Get(category ?? "linq2db");

                    switch (lvl)
                    {
                        case TraceLevel.Error:
                            logger.Error(msg ?? string.Empty);
                            break;
                        case TraceLevel.Warning:
                            logger.Warn(msg ?? string.Empty);
                            break;
                        // all other levels are handled as Debug by design (at the moment linq2db traces with Info level)
                        default:
                            logger.Debug(msg ?? string.Empty);
                            break;
                    }
                }
            );
    }
}
