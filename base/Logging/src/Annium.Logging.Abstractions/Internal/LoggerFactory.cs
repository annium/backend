using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Logging.Abstractions.Internal
{
    internal class LoggerFactory : ILoggerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public LoggerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ILogger<T> GetLogger<T>() => _serviceProvider.GetRequiredService<ILogger<T>>();
    }
}