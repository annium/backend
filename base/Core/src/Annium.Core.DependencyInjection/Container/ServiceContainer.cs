using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class ServiceContainer : IServiceContainer
    {
        public int Count => Collection.Count;
        public IServiceCollection Collection { get; }

        public ServiceContainer() : this(new ServiceCollection())
        {
        }

        public ServiceContainer(IServiceCollection collection)
        {
            Collection = collection;
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator() => Collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();

        public IServiceContainer Add(ServiceDescriptor item)
        {
            Framework.Log(() => $"{item.ToReadableString()}");
            Collection.Add(item);

            return this;
        }

        public IServiceContainer TryAdd(ServiceDescriptor item)
        {
            // skip if descriptor has ImplementationType and it is already registered
            if (
                item.ImplementationType != null &&
                Collection.Any(x =>
                    x.ServiceType == item.ServiceType &&
                    x.ImplementationType == item.ImplementationType
                )
            )
            {
                Framework.Log(() => $"{item.ToReadableString()} - skip, implementation type is already registered");

                return this;
            }

            // skip if descriptor has ImplementationInstance and it is already registered
            if (
                item.ImplementationInstance != null &&
                Collection.Any(x =>
                    x.ServiceType == item.ServiceType &&
                    x.ImplementationInstance?.Equals(item.ImplementationInstance) == true
                )
            )
            {
                Framework.Log(() => $"{item.ToReadableString()} - skip, implementation instance is already registered");

                return this;
            }

            Framework.Log(() => $"{item.ToReadableString()} - add");
            Collection.Add(item);

            return this;
        }

        public IServiceContainer Clear()
        {
            Framework.Log();
            Collection.Clear();

            return this;
        }

        public bool Contains(ServiceDescriptor item) => Collection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => Collection.CopyTo(array, arrayIndex);

        public bool Remove(ServiceDescriptor item) => Collection.Remove(item);
    }
}