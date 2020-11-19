using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Units
{
    internal class InstanceRegistrationUnit : IInstanceRegistrationUnit
    {
        public Type Type { get; }
        public IReadOnlyCollection<IServiceDescriptor> Descriptors { get; }

        public void As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public void AsFactory(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public void AddKey<TKey>(TKey key)
        {
            throw new NotImplementedException();
        }
    }
}