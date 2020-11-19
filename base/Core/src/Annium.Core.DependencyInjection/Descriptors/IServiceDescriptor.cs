using System;

namespace Annium.Core.DependencyInjection
{
    public interface IServiceDescriptor
    {
        public ServiceLifetime Lifetime { get; }

        public Type ServiceType { get; }
    }
}