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

        public StorageBase CreateStorage(ConfigurationBase configuration)
        {
            var factoryType = typeof(Func<,>).MakeGenericType(configuration.GetType(), typeof(StorageBase));

            var factory = (Delegate) provider.GetRequiredService(factoryType);

            try
            {
                var storage = (StorageBase) factory.DynamicInvoke(configuration);

                return storage;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}