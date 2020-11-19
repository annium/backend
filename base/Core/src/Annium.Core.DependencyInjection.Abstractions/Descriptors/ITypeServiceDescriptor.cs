using System;

namespace Annium.Core.DependencyInjection
{
    public interface ITypeServiceDescriptor : IServiceDescriptor
    {
        public Type ImplementationType { get; }
    }
}