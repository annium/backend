using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Storage.Abstractions
{
    internal class StorageFactory : IStorageFactory
    {
        private readonly IServiceProvider _provider;

        public StorageFactory(
            IServiceProvider provider
        )
        {
            _provider = provider;
        }

        public IStorage CreateStorage(ConfigurationBase configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var factoryType = typeof(Func<,>).MakeGenericType(configuration.GetType(), typeof(IStorage));

            var factory = (Delegate) _provider.GetRequiredService(factoryType);

            try
            {
                return (IStorage) factory.DynamicInvoke(configuration) !;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        }
    }
}