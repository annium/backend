using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class ServiceContainer : IServiceContainer
    {
        public IServiceCollection Collection { get; }
        public IBulkRegistrationBuilderBase Add(IEnumerable<Type> types) => new BulkRegistrationBuilder(types, Add);
        public IInstanceRegistrationBuilderBase Add<T>(T instance) where T : notnull => new InstanceRegistrationBuilder(typeof(T), instance, Add);
        public ISingleRegistrationBuilderBase Add(Type type) => new SingleRegistrationBuilder(type, Add);
        public IBulkRegistrationBuilderBase TryAdd(IEnumerable<Type> types) => new BulkRegistrationBuilder(types, TryAdd);
        public IInstanceRegistrationBuilderBase TryAdd<T>(T instance) where T : notnull => new InstanceRegistrationBuilder(typeof(T), instance, TryAdd);
        public ISingleRegistrationBuilderBase TryAdd(Type type) => new SingleRegistrationBuilder(type, TryAdd);

        public bool Contains(IServiceDescriptor descriptor)
        {
            var lifetime = (Microsoft.Extensions.DependencyInjection.ServiceLifetime) descriptor.Lifetime;

            return descriptor switch
            {
                ITypeServiceDescriptor d => Collection
                    .Any(x => x.Lifetime == lifetime && x.ServiceType == d.ServiceType && x.ImplementationType == d.ImplementationType),
                IFactoryServiceDescriptor d => Collection
                    .Any(x => x.Lifetime == lifetime && x.ServiceType == d.ServiceType && x.ImplementationFactory == d.ImplementationFactory),
                IInstanceServiceDescriptor d => Collection
                    .Any(x => x.Lifetime == lifetime && x.ServiceType == d.ServiceType && x.ImplementationInstance == d.ImplementationInstance),
                _ => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
            };
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

        private void Add(IServiceDescriptor item)
        {
            Framework.Log(() => $"{item.ToReadableString()}");
            Collection.Add(item.ToMicrosoft());
        }

        private void TryAdd(IServiceDescriptor item)
        {
            if (Contains(item))
            {
                Framework.Log(() => $"{item.ToReadableString()} - skip, is already registered");

                return;
            }

            Framework.Log(() => $"{item.ToReadableString()} - add");
            Collection.Add(item.ToMicrosoft());
        }
    }
}