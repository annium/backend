using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection
{
    public interface IRegistrationUnit
    {
        Type Type { get; }
        IReadOnlyCollection<IServiceDescriptor> Descriptors { get; }
        void As(Type serviceType);
        void AsFactory(Type serviceType);
        void AddKey<TKey>(TKey key);
    }
}