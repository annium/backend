using System;
using Annium.Core.DependencyInjection;

namespace Annium.Logging.Shared;

public class LogRoute<TContext>
    where TContext : class, ILogContext
{
    internal Func<LogMessage<TContext>, bool> Filter { get; private set; } = _ => true;
    internal IServiceDescriptor? Service { get; private set; }
    internal LogRouteConfiguration? Configuration { get; private set; }
    private readonly Action<LogRoute<TContext>> _registerRoute;

    internal LogRoute(Action<LogRoute<TContext>> registerRoute)
    {
        _registerRoute = registerRoute;

        registerRoute(this);
    }

    public LogRoute<TContext> For(Func<LogMessage<TContext>, bool> filter) => new(_registerRoute) { Filter = filter };

    public LogRoute<TContext> UseType<T>(LogRouteConfiguration configuration)
        where T : class, ILogHandler<TContext>
        => Use(ServiceDescriptor.Type<T, T>(ServiceLifetime.Singleton), configuration);

    public LogRoute<TContext> UseInstance<T>(T instance, LogRouteConfiguration configuration)
        where T : class, ILogHandler<TContext>
        => Use(ServiceDescriptor.Instance(instance, ServiceLifetime.Singleton), configuration);

    public LogRoute<TContext> UseFactory<T>(Func<IServiceProvider, T> factory, LogRouteConfiguration configuration)
        where T : class, ILogHandler<TContext>
        => Use(ServiceDescriptor.Factory(factory, ServiceLifetime.Singleton), configuration);

    public LogRoute<TContext> UseAsyncType<T>(LogRouteConfiguration configuration)
        where T : class, IAsyncLogHandler<TContext>
        => Use(ServiceDescriptor.Type<T, T>(ServiceLifetime.Singleton), configuration);

    public LogRoute<TContext> UseAsyncInstance<T>(T instance, LogRouteConfiguration configuration)
        where T : class, IAsyncLogHandler<TContext>
        => Use(ServiceDescriptor.Instance(instance, ServiceLifetime.Singleton), configuration);

    public LogRoute<TContext> UseAsyncFactory<T>(Func<IServiceProvider, T> factory, LogRouteConfiguration configuration)
        where T : class, IAsyncLogHandler<TContext>
        => Use(ServiceDescriptor.Factory(factory, ServiceLifetime.Singleton), configuration);

    private LogRoute<TContext> Use(IServiceDescriptor service, LogRouteConfiguration configuration)
    {
        Service = service;
        Configuration = configuration;

        return this;
    }
}