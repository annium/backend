using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Units
{
    internal class InstanceRegistrationUnit
    {
        public Type Type { get; }
        public object Instance { get; }
        public IReadOnlyCollection<IServiceDescriptor> Descriptors => _descriptors;
        private readonly List<IServiceDescriptor> _descriptors = new();

        public InstanceRegistrationUnit(Type type, object instance)
        {
            Type = type;
            Instance = instance;
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