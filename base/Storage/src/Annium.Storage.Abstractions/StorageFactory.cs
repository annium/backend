using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Storage.Abstractions
{
    internal class StorageFactory : IStorageFactory
    {
        private readonly IServiceProvider provider;

        public StorageFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public IStorage CreateStorage(ConfigurationBase configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var factoryType = typeof(Func<,>).MakeGenericType(configuration.GetType(), typeof(IStorage));

            var factory = (Delegate) provider.GetRequiredService(factoryType);

            try
            {
                var storage = (IStorage) factory.DynamicInvoke(configuration);

                return storage;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}