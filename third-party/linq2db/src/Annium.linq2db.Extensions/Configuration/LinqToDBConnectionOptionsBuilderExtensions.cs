using System;
using System.Diagnostics;
using Annium.Core.DependencyInjection;
using Annium.linq2db.Extensions.Models;
using Annium.Logging.Abstractions;
using LinqToDB.Configuration;

namespace Annium.linq2db.Extensions.Configuration;

public static class LinqToDBConnectionOptionsBuilderExtensions
{
    public static LinqToDBConnectionOptionsBuilder UseLogging<TConnection>(this LinqToDBConnectionOptionsBuilder builder, IServiceProvider sp)
        where TConnection : DataConnectionBase, ILogSubject<TConnection>
    {
        var logSubject = sp.Resolve<ILogSubject<TConnection>>();

        return builder
            .WithTraceLevel(TraceLevel.Verbose)
            .WriteTraceWith((msg, category, lvl) => logSubject.Log().Log(MapTraceLevel(lvl), $"{category}: {msg}"));

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