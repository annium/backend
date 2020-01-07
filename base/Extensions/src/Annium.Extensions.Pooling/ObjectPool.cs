using System;
using System.Threading;
using Annium.Extensions.Pooling.Loaders;
using Annium.Extensions.Pooling.Storages;

namespace Annium.Extensions.Pooling
{
    public class ObjectPool<T> : IObjectPool<T>, IDisposable
    {
        private readonly object poolLocker = new object();
        private readonly ILoader<T> loader;
        private readonly IStorage<T> storage;
        private readonly Semaphore semaphore;

        public ObjectPool(
            Func<T> factory,
            int capacity,
            PoolLoadMode loadMode = PoolLoadMode.Lazy,
            PoolStorageMode storageMode = PoolStorageMode.Fifo
        )
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, $"Argument '{nameof(capacity)}' must be greater than zero.");

            storage = StorageFactory.Create<T>(storageMode, capacity);
            loader = LoaderFactory.Create(loadMode, factory, storage);
            semaphore = new Semaphore(capacity, capacity);
        }

        public T Get()
        {
            semaphore.WaitOne();
            lock (poolLocker)
                return loader.Get();
        }

        public void Return(T item)
        {
            lock (poolLocker)
                storage.Return(item);
            semaphore.Release();
        }

        #region IDisposable support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
            {
                if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                    storage.Dispose();
                semaphore.Close();
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}