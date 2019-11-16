using System;
using System.Collections.Generic;

namespace Annium.Extensions.Pooling.Storages
{
    internal class QueueStorage<T> : StorageBase<T>, IStorage<T>
    {
        private readonly Queue<T> freeItems;
        private readonly List<T> usedItems;

        public QueueStorage(int capacity) : base(capacity)
        {
            freeItems = new Queue<T>(capacity);
            usedItems = new List<T>(capacity);
        }

        protected override void Register(T item) => freeItems.Enqueue(item);

        protected override T Acquire()
        {
            var item = freeItems.Dequeue();
            usedItems.Add(item);

            return item;
        }

        protected override bool Release(T item)
        {
            var released = usedItems.Remove(item);
            if (!released)
                return false;

            freeItems.Enqueue(item);

            return true;
        }

        protected override void DisposeInternal()
        {
            foreach (var item in freeItems)
                (item as IDisposable)!.Dispose();
            freeItems.Clear();

            foreach (var item in usedItems)
                (item as IDisposable)!.Dispose();
            usedItems.Clear();
        }
    }
}