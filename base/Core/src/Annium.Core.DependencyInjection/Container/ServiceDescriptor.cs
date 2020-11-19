using System;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceDescriptor
    {
        public static IServiceDescriptor From(MicrosoftServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType is not null)
                return Type(descriptor.ServiceType, descriptor.ImplementationType, (ServiceLifetime) descriptor.Lifetime);

            if (descriptor.ImplementationFactory is not null)
                return Factory(descriptor.ServiceType, descriptor.ImplementationFactory, (ServiceLifetime) descriptor.Lifetime);

            if (descriptor.ImplementationInstance is not null)
                return Instance(descriptor.ServiceType, descriptor.ImplementationInstance, (ServiceLifetime) descriptor.Lifetime);

            throw new NotSupportedException($"{descriptor} has unsupported configuration");
        }

        public static ITypeServiceDescriptor Type(Type serviceType, Type implementationType, ServiceLifetime lifetime)
            => new TypeServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = lifetime,
            };

        public static IFactoryServiceDescriptor Factory(Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime)
            => new FactoryServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationFactory = implementationFactory,
                Lifetime = lifetime,
            };

        public static IInstanceServiceDescriptor Instance(Type serviceType, object implementationInstance, ServiceLifetime lifetime)
            => new InstanceServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationInstance = implementationInstance,
                Lifetime = lifetime
            };
    }

    internal sealed record TypeServiceDescriptor : ITypeServiceDescriptor
    {
        public ServiceLifetime Lifetime { get; init; }
        public Type ServiceType { get; init; }
        public Type ImplementationType { get; init; }
    }

    internal sealed record FactoryServiceDescriptor : IFactoryServiceDescriptor
    {
        public ServiceLifetime Lifetime { get; init; }
        public Type ServiceType { get; init; }
        public Func<IServiceProvider, object> ImplementationFactory { get; init; }
    }

    internal sealed record InstanceServiceDescriptor : IInstanceServiceDescriptor
    {
        public ServiceLifetime Lifetime { get; init; }
        public Type ServiceType { get; init; }
        public object ImplementationInstance { get; init; }
    }
}