using System;
using Annium.Logging.Shared;
using Annium.Logging.XUnit.Internal;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LogRouteExtensions
{
    public static LogRoute<TContext> UseTestOutput<TContext>(
        this LogRoute<TContext> route
    )
        where TContext : class, ILogContext
        => route.UseTestOutput(LogMessageExtensions.DefaultFormat);

    public static LogRoute<TContext> UseTestOutput<TContext>(
        this LogRoute<TContext> route,
        Func<LogMessage<TContext>, string> format
    )
        where TContext : class, ILogContext
    {
        route.UseFactory(sp => new XunitLogHandler<TContext>(sp.Resolve<ITestOutputHelper>(), format), new LogRouteConfiguration());

        return route;
    }
}