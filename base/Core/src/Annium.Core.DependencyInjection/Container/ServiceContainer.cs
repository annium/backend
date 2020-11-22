using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders;
using Annium.Core.DependencyInjection.Internal.Container;
using Annium.Core.Internal;
using Annium.Core.Primitives;
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
            AddInternalTypes();
        }

        public IBulkRegistrationBuilderBase Add(IEnumerable<Type> types) =>
            new BulkRegistrationBuilder(types, Register);

        public IFactoryRegistrationBuilderBase Add(Type type, Func<IServiceProvider, object> factory) =>
            new FactoryRegistrationBuilder(type, factory, Register);

        public IInstanceRegistrationBuilderBase Add<T>(T instance) where T : class =>
            new InstanceRegistrationBuilder(typeof(T), instance, Register);

        public ISingleRegistrationBuilderBase Add(Type type) =>
            new SingleRegistrationBuilder(type, Register);

        public bool Contains(IServiceDescriptor descriptor)
        {
            var lifetime = (Microsoft.Extensions.DependencyInjection.ServiceLifetime) descriptor.Lifetime;

            return descriptor switch
            {
                ITypeServiceDescriptor d => Collection
                    .Any(x =>
                        x.Lifetime == lifetime &&
                        x.ServiceType == d.ServiceType &&
                        x.ImplementationType == d.ImplementationType
                    ),
                IFactoryServiceDescriptor d => Collection
                    .Any(x =>
                        x.Lifetime == lifetime &&
                        x.ServiceType == d.ServiceType &&
                        x.ImplementationFactory?.Method == d.ImplementationFactory.Method &&
                        x.ImplementationFactory?.Target == d.ImplementationFactory.Target
                    ),
                IInstanceServiceDescriptor d => Collection
                    .Any(x =>
                        x.Lifetime == lifetime &&
                        x.ServiceType == d.ServiceType &&
                        x.ImplementationInstance == d.ImplementationInstance
                    ),
                _ => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
            };
        }

        public IEnumerator<IServiceDescriptor> GetEnumerator() => Collection.Select(ServiceDescriptor.From).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();

        private void Register(IServiceDescriptor item)
        {
            if (Contains(item))
            {
                Framework.Log(() => $"{item.ToReadableString()} - skip, is already registered");

                return;
            }

            Framework.Log(() => $"{item.ToReadableString()} - add");
            Collection.Add(item.ToMicrosoft());
        }

        private void AddInternalTypes()
        {
            Register(ServiceDescriptor.Type(typeof(IIndex<,>), typeof(Index<,>), ServiceLifetime.Transient));
        }
    }
}