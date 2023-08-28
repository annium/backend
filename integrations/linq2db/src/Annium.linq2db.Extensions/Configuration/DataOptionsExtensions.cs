using System;
using System.Diagnostics;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using LinqToDB;
using LinqToDB.Data;

namespace Annium.linq2db.Extensions.Configuration;

// ReSharper disable once InconsistentNaming
public static class DataOptionsExtensions
{
    public static DataOptions UseLogging<TConnection>(this DataOptions options, IServiceProvider sp)
        where TConnection : DataConnection, ILogSubject
    {
        var logSubject = sp.Resolve<ILogSubject>();

        return options
            .UseTraceLevel(TraceLevel.Verbose)
            .UseTraceWith((msg, category, lvl) => logSubject.Log().Log(MapTraceLevel(lvl), $"{category}: {msg}"));

        static LogLevel MapTraceLevel(TraceLevel level) => level switch
        {
            TraceLevel.Error   => LogLevel.Error,
            TraceLevel.Warning => LogLevel.Warn,
            TraceLevel.Info    => LogLevel.Info,
            TraceLevel.Verbose => LogLevel.Trace,
            _                  => LogLevel.None
        };
    }
}