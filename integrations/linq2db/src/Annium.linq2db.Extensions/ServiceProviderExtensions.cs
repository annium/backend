using System;
using System.Diagnostics;
using Annium.Logging.Abstractions;
using LinqToDB.Data;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static IServiceProvider UseConnectionTracing<TConnection>(
        this IServiceProvider provider
    )
        where TConnection : DataConnection
    {
        DataConnection.TurnTraceSwitchOn();
        DataConnection.WriteTraceLine = (message, displayName, traceLevel) => Console.WriteLine($"{MapTraceLevel(traceLevel)} {message} {displayName}");

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