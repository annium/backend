using Annium.Extensions.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Tests
{
    internal static class ServiceDescriptorExtensions
    {
        public static string FriendlyName(this ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
                return $"{descriptor.ServiceType.FriendlyName()} -> Type {descriptor.ImplementationType.FriendlyName()}";

            if (descriptor.ImplementationFactory != null)
                return $"{descriptor.ServiceType.FriendlyName()} -> Factory {descriptor.ImplementationFactory.GetType().FriendlyName()}";

            return $"{descriptor.ServiceType.FriendlyName()} -> Instance {descriptor.ImplementationInstance!.GetType().FriendlyName()}";
        }
    }
}