using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public interface IServiceContainer : IEnumerable<IServiceDescriptor>
    {
        int Count { get; }
        IServiceCollection Collection { get; }
        IBulkRegistrationBuilderBase Add(IEnumerable<Type> types);
        IFactoryRegistrationBuilderBase Add<T>(Func<IServiceProvider, T> factory) where T : class;
        IInstanceRegistrationBuilderBase Add<T>(T instance) where T : class;
        ISingleRegistrationBuilderBase Add(Type type);
        bool Contains(IServiceDescriptor descriptor);
    }
}