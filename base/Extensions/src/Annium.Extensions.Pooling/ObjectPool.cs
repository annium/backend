using System;
using System.Threading;
using Annium.Extensions.Pooling.Loaders;
using Annium.Extensions.Pooling.Storages;

namespace Annium.Extensions.Pooling
{
    public class ObjectPool<T> : IObjectPool<T>, IDisposable
    {
        private readonly object _poolLocker = new();
        private readonly ILoader<T> _loader;
        private readonly IStorage<T> _storage;
        private readonly Semaphore _semaphore;

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

            _storage = StorageFactory.Create<T>(storageMode, capacity);
            _loader = LoaderFactory.Create(loadMode, factory, _storage);
            _semaphore = new Semaphore(capacity, capacity);
        }

        public T Get()
        {
            _semaphore.WaitOne();
            lock (_poolLocker)
                return _loader.Get();
        }

        public void Return(T item)
        {
            lock (_poolLocker)
                _storage.Return(item);
            _semaphore.Release();
        }

        #region IDisposable support

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing)
            {
                if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                    _storage.Dispose();
                _semaphore.Close();
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}