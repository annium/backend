using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Units
{
    internal class BulkRegistrationUnit : IBulkRegistrationUnit
    {
        public Type Type { get; }
        public IReadOnlyCollection<IServiceDescriptor> Descriptors => _descriptors;
        private readonly List<IServiceDescriptor> _descriptors = new();

        public BulkRegistrationUnit(Type type)
        {
            Type = type;
        }

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