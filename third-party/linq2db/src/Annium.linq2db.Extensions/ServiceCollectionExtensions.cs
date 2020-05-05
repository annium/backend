using System;
using System.Diagnostics;
using Annium.linq2db.Extensions;
using Annium.Logging.Abstractions;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static IServiceProvider UseConnectionTracing<TConnection>(
            this IServiceProvider provider
        )
            where TConnection : DataConnectionBase
        {
            var logger = provider.GetRequiredService<ILogger<TConnection>>();
            DataConnection.TurnTraceSwitchOn();
            DataConnection.WriteTraceLine = (message, displayName, traceLevel) => logger.Log(MapTraceLevel(traceLevel), $"{message} {displayName}");

            return provider;
        }

        private static LogLevel MapTraceLevel(TraceLevel level) => level switch
        {
            TraceLevel.Error   => LogLevel.Error,
            TraceLevel.Warning => LogLevel.Warn,
            TraceLevel.Info    => LogLevel.Info,
            TraceLevel.Verbose => LogLevel.Trace,
            _                  => LogLevel.None,
        };
    }
}