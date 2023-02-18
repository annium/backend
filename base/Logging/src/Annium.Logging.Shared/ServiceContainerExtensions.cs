using System.Collections.Generic;
using Annium.Logging.Abstractions;
using Annium.Logging.Shared;
using Annium.Logging.Shared.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddLogging<TContext>(
        this IServiceContainer container
    )
        where TContext : class, ILogContext
    {
        return container.AddLoggingBase<TContext>();
    }

    public static IServiceContainer AddLogging(
        this IServiceContainer container
    )
    {
        return container.AddLoggingBase<DefaultLogContext>();
    }

    private static IServiceContainer AddLoggingBase<TContext>(
        this IServiceContainer container
    )
        where TContext : class, ILogContext
    {
        container.Add(new List<ILogScheduler<TContext>>()).AsSelf().As<IReadOnlyCollection<ILogScheduler<TContext>>>().Singleton();

        container.Add<TContext>().AsSelf().In(ServiceLifetime.Scoped);
        container.Add(typeof(Logger<>)).As(typeof(ILogger<>)).In(ServiceLifetime.Scoped);
        container.Add(typeof(LogSubject<>)).As(typeof(ILogSubject<>)).In(ServiceLifetime.Scoped);
        container.Add<ILoggerFactory, LoggerFactory>().In(ServiceLifetime.Scoped);
        container.Add<LogRouter<TContext>>().AsSelf().Singleton();
        container.Add<ILogSentry<TContext>, LogSentry<TContext>>().Singleton();
        container.Add<ILogSentryBridge, LogSentryBridge<TContext>>().In(ServiceLifetime.Scoped);
        container.AddProfile(p =>
        {
            p.Map<LogLevel, string>(x => x.ToString());
            p.Map<string, LogLevel>(x => x.ParseEnum<LogLevel>());
        });

        container.OnBuild += sp => sp.Resolve<LogRouter<TContext>>();

        return container;
    }
}