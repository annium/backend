using System;

namespace Annium.Core.DependencyInjection
{
    public interface IFactoryServiceDescriptor : IServiceDescriptor
    {
        public Func<IServiceProvider, object> ImplementationFactory { get; }
    }
}