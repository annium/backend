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
            ITypeServiceDescriptor x     => $"type {x.ServiceType.FriendlyName()} -> {x.ImplementationType.FriendlyName()}",
            IFactoryServiceDescriptor x  => $"factory {x.ServiceType.FriendlyName()} ->  {x.ImplementationFactory.GetType().FriendlyName()}",
            IInstanceServiceDescriptor x => $"instance {x.ServiceType.FriendlyName()} -> {x.ImplementationInstance.GetType().FriendlyName()}",
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