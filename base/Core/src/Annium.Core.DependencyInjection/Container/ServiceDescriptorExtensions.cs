using Annium.Core.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceDescriptorExtensions
    {
        public static string ToReadableString(this ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType is not null)
                return $"{descriptor.ServiceType.FriendlyName()} -> {descriptor.ImplementationType} type";

            if (descriptor.ImplementationFactory is not null)
                return $"{descriptor.ServiceType} -> {descriptor.ImplementationFactory.GetType()} factory";

            return $"{descriptor.ServiceType} -> {descriptor.ImplementationInstance!.GetType()} instance";
        }
    }
}