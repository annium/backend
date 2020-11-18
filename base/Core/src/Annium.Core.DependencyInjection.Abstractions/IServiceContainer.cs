using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public interface IServiceContainer : IEnumerable<IServiceDescriptor>
    {
        ISingleRegistrationBuilderBase Add(Type type);
        IInstanceRegistrationBuilderBase Add<T>(T instance);
        IBulkRegistrationBuilderBase Add(Type[] types);
        ISingleRegistrationBuilderBase TryAdd(Type type);
        IInstanceRegistrationBuilderBase TryAdd<T>(T instance);
        IBulkRegistrationBuilderBase TryAdd(Type[] types);
        bool Contains(IServiceDescriptor item);
        IServiceCollection Collection { get; }
    }
}