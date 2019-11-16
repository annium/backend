using System;
using Annium.Extensions.Pooling.Storages;

namespace Annium.Extensions.Pooling.Loaders
{
    internal class EagerLoader<T> : ILoader<T>
    {
        private readonly Func<T> factory;
        private readonly IStorage<T> storage;

        public EagerLoader(
            Func<T> factory,
            IStorage<T> storage
        )
        {
            this.factory = factory;
            this.storage = storage;
        }

        public T Get()
        {
            // if not all created yet - create first
            if (storage.Free + storage.Used < storage.Capacity)
            {
                var item = factory();
                storage.Add(item);
            }

            return storage.Get();
        }
    }
}