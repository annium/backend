using System;
using System.Diagnostics;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using LinqToDB;

namespace Annium.linq2db.Extensions.Configuration;

/// <summary>
/// Extension methods for configuring linq2db DataOptions
/// </summary>
public static class DataOptionsExtensions
{
    /// <summary>
    /// Configures logging for linq2db using the Annium logging framework
    /// </summary>
    /// <param name="options">The DataOptions to configure</param>
    /// <param name="sp">The service provider for resolving logging dependencies</param>
    /// <returns>The configured DataOptions for chaining</returns>
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
