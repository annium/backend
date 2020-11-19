using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public interface IServiceContainer : IEnumerable<IServiceDescriptor>
    {
        IServiceCollection Collection { get; }
        IBulkRegistrationBuilderBase Add(IEnumerable<Type> types);
        IInstanceRegistrationBuilderBase Add<T>(T instance) where T : notnull;
        ISingleRegistrationBuilderBase Add(Type type);
        IBulkRegistrationBuilderBase TryAdd(IEnumerable<Type> types);
        IInstanceRegistrationBuilderBase TryAdd<T>(T instance) where T : notnull;
        ISingleRegistrationBuilderBase TryAdd(Type type);
        bool Contains(IServiceDescriptor descriptor);
    }
}