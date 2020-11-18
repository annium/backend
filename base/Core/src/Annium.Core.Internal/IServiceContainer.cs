using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public interface IServiceContainer : IEnumerable<ServiceDescriptor>
    {
        int Count { get; }
        IServiceContainer Add(ServiceDescriptor item);
        IServiceContainer TryAdd(ServiceDescriptor item);
        IServiceContainer Clear();
        bool Contains(ServiceDescriptor item);
        bool Remove(ServiceDescriptor item);
        IServiceCollection Collection { get; }
    }
}