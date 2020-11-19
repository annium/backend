using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class ServiceContainer : IServiceContainer
    {
        public IServiceCollection Collection { get; }

        public ISingleRegistrationBuilderBase Add(Type type)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase Add<T>(T instance)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderBase Add(IEnumerable<Type> types)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase TryAdd(Type type)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase TryAdd<T>(T instance)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderBase TryAdd(IEnumerable<Type> types)
        {
            throw new NotImplementedException();
        }

        public bool Contains(IServiceDescriptor item)
        {
            throw new NotImplementedException();
        }


        public ServiceContainer() : this(new ServiceCollection())
        {
        }

        public ServiceContainer(IServiceCollection collection)
        {
            Collection = collection;
        }

        public IEnumerator<IServiceDescriptor> GetEnumerator() => Collection.Select(ServiceDescriptor.From).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();

        internal IServiceContainer Add(IServiceDescriptor item)
        {
            Framework.Log(() => $"{item.ToReadableString()}");
            Collection.Add(item.ToMicrosoft());

            return this;
        }

        public IServiceContainer TryAdd(IServiceDescriptor item)
        {
            // skip if it's ITypeServiceDescriptor and ImplementationType is already registered
            if (
                item is ITypeServiceDescriptor typeDescriptor &&
                Collection.Any(x =>
                    x.ServiceType == typeDescriptor.ServiceType &&
                    x.ImplementationType == typeDescriptor.ImplementationType
                )
            )
            {
                Framework.Log(() => $"{item.ToReadableString()} - skip, implementation type is already registered");

                return this;
            }

            // skip if it's ITypeServiceDescriptor and ImplementationType is already registered
            // skip if descriptor has ImplementationInstance and it is already registered
            if (
                item is IInstanceServiceDescriptor instanceDescriptor &&
                Collection.Any(x =>
                    x.ServiceType == instanceDescriptor.ServiceType &&
                    x.ImplementationInstance?.Equals(instanceDescriptor.ImplementationInstance) == true
                )
            )
            {
                Framework.Log(() => $"{item.ToReadableString()} - skip, implementation instance is already registered");

                return this;
            }

            Framework.Log(() => $"{item.ToReadableString()} - add");
            Collection.Add(item.ToMicrosoft());

            return this;
        }
    }
}