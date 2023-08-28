using System;
using Annium.Core.DependencyInjection;

namespace Annium.Logging.Shared.Internal;

internal class LoggerFactory : ILoggerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LoggerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILogger Get<T>() => _serviceProvider.Resolve<ILogger>();
}