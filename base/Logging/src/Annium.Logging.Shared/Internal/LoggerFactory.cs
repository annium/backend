using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal;

internal class LoggerFactory : ILoggerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LoggerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILogger<T> GetLogger<T>() => _serviceProvider.Resolve<ILogger<T>>();
}