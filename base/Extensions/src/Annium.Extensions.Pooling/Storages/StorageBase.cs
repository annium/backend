using System;

namespace Annium.Extensions.Pooling.Storages
{
    internal abstract class StorageBase<T> : IDisposable
    {
        public int Capacity { get; }
        public int Free { get; private set; }
        public int Used { get; private set; }

        protected StorageBase(int capacity)
        {
            Capacity = capacity;
        }

        public void Add(T item)
        {
            if (Free == Capacity)
                throw new InvalidOperationException("Storage capacity reached");

            Register(item);
            ++Free;
        }

        public T Get()
        {
            if (Free == 0)
                throw new InvalidOperationException("No free slots.");

            var item = Acquire();
            --Free;
            ++Used;

            return item;
        }

        public void Return(T item)
        {
            if (!Release(item))
                throw new InvalidOperationException("Item is not registered in storage.");

            ++Free;
            --Used;
        }

        protected abstract void Register(T item);
        protected abstract T Acquire();
        protected abstract bool Release(T item);
        protected abstract void DisposeInternal();

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
            {
                DisposeInternal();
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