using System.Collections;
using System.Collections.Generic;
using Annium.Core.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class ServiceContainer : IServiceContainer
    {
        public int Count => Collection.Count;
        public bool IsReadOnly => Collection.IsReadOnly;
        public IServiceCollection Collection { get; }

        public ServiceContainer() : this(new ServiceCollection())
        {
        }

        public ServiceContainer(IServiceCollection collection)
        {
            Collection = collection;
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator() => Collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();

        public void Add(ServiceDescriptor item)
        {
            Framework.Log(() => $"{item.ToReadableString()}");
            Collection.Add(item);
        }

        public void Clear()
        {
            Framework.Log();
            Collection.Clear();
        }

        public bool Contains(ServiceDescriptor item) => Collection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => Collection.CopyTo(array, arrayIndex);

        public bool Remove(ServiceDescriptor item) => Collection.Remove(item);
    }
}