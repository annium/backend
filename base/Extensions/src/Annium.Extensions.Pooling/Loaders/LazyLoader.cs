using System;
using Annium.Extensions.Pooling.Storages;

namespace Annium.Extensions.Pooling.Loaders
{
    internal class LazyLoader<T> : ILoader<T>
    {
        private readonly Func<T> factory;
        private readonly IStorage<T> storage;

        public LazyLoader(
            Func<T> factory,
            IStorage<T> storage
        )
        {
            this.factory = factory;
            this.storage = storage;
        }

        public T Get()
        {
            // if any free - try use first
            if (storage.Free > 0)
                return storage.Get();

            // if not all created yet - create
            if (storage.Used < storage.Capacity)
            {
                var item = factory();
                storage.Add(item);
            }

            return storage.Get();
        }
    }
}