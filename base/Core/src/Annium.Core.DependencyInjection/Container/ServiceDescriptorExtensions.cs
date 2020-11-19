using System;
using Annium.Core.Primitives;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceDescriptorExtensions
    {
        public static string ToReadableString(this IServiceDescriptor descriptor) => descriptor switch
        {
            ITypeServiceDescriptor x     => $"{x.ServiceType.FriendlyName()} -> {x.ImplementationType} type",
            IFactoryServiceDescriptor x  => $"{x.ServiceType} -> {x.ImplementationFactory.GetType()} factory",
            IInstanceServiceDescriptor x => $"{x.ServiceType} -> {x.ImplementationInstance.GetType()} instance",
            _                            => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
        };

        public static MicrosoftServiceDescriptor ToMicrosoft(this IServiceDescriptor descriptor) => descriptor switch
        {
            ITypeServiceDescriptor x     => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationType, (MicrosoftServiceLifetime) x.Lifetime),
            IFactoryServiceDescriptor x  => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationFactory, (MicrosoftServiceLifetime) x.Lifetime),
            IInstanceServiceDescriptor x => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationInstance),
            _                            => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
        };

    }
}