using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public interface IServiceContainer : IEnumerable<IServiceDescriptor>
    {
        IServiceCollection Collection { get; }
        IBulkRegistrationBuilderBase Add(IEnumerable<Type> types);
        IFactoryRegistrationBuilderBase Add<T>(Func<IServiceProvider, T> factory) where T : notnull;
        IInstanceRegistrationBuilderBase Add<T>(T instance) where T : notnull;
        ISingleRegistrationBuilderBase Add(Type type);
        IBulkRegistrationBuilderBase TryAdd(IEnumerable<Type> types);
        IFactoryRegistrationBuilderBase TryAdd<T>(Func<IServiceProvider, T> factory) where T : notnull;
        IInstanceRegistrationBuilderBase TryAdd<T>(T instance) where T : notnull;
        ISingleRegistrationBuilderBase TryAdd(Type type);
        bool Contains(IServiceDescriptor descriptor);
    }
}