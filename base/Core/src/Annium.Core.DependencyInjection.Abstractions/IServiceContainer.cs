using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public interface IServiceContainer : IEnumerable<IServiceDescriptor>
    {
        IServiceCollection Collection { get; }
        ISingleRegistrationBuilderBase Add(Type type);
        IInstanceRegistrationBuilderBase Add<T>(T instance);
        IBulkRegistrationBuilderBase Add(IEnumerable<Type> types);
        ISingleRegistrationBuilderBase TryAdd(Type type);
        IInstanceRegistrationBuilderBase TryAdd<T>(T instance);
        IBulkRegistrationBuilderBase TryAdd(IEnumerable<Type> types);
        bool Contains(IServiceDescriptor item);
    }
}