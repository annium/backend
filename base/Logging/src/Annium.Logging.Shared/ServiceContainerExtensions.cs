using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Logging.Shared;
using Annium.Logging.Shared.Internal;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddLogging<TContext>(
        this IServiceContainer container
    )
        where TContext : class, ILogContext
    {
        return container.AddLoggingBase<TContext>(ServiceLifetime.Scoped);
    }

    public static IServiceContainer AddLogging(
        this IServiceContainer container
    )
    {
        return container.AddLoggingBase<DefaultLogContext>(ServiceLifetime.Singleton);
    }

    private static IServiceContainer AddLoggingBase<TContext>(
        this IServiceContainer container,
        ServiceLifetime lifetime
    )
        where TContext : class, ILogContext
    {
        container.Add(new List<ILogScheduler<TContext>>()).AsSelf().As<IReadOnlyCollection<ILogScheduler<TContext>>>().Singleton();

        container.Add<TContext>().AsSelf().In(lifetime);
        container.Add(typeof(Logger<>)).As(typeof(ILogger<>)).In(lifetime);
        container.Add(typeof(LogSubject<>)).As(typeof(ILogSubject<>)).In(lifetime);
        container.Add<ILoggerFactory, LoggerFactory>().In(lifetime);
        container.Add<LogRouter<TContext>>().AsSelf().Singleton();
        container.Add<ILogSentry<TContext>, LogSentry<TContext>>().Singleton();
        container.Add<ILogSentryBridge, LogSentryBridge<TContext>>().In(lifetime);
        container.AddProfile(p =>
        {
            p.Map<LogLevel, string>(x => x.ToString());
            p.Map<string, LogLevel>(x => x.ParseEnum<LogLevel>());
        });

        container.OnBuild += sp => sp.Resolve<LogRouter<TContext>>();

        return container;
    }
}