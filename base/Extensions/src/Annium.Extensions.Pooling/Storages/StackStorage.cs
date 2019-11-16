using System;
using System.Collections.Generic;

namespace Annium.Extensions.Pooling.Storages
{
    internal class StackStorage<T> : StorageBase<T>, IStorage<T>
    {
        private readonly Stack<T> freeItems;
        private readonly List<T> usedItems;

        public StackStorage(int capacity) : base(capacity)
        {
            freeItems = new Stack<T>(capacity);
            usedItems = new List<T>(capacity);
        }

        protected override void Register(T item) => freeItems.Push(item);

        protected override T Acquire()
        {
            var item = freeItems.Pop();
            usedItems.Add(item);

            return item;
        }

        protected override bool Release(T item)
        {
            var released = usedItems.Remove(item);
            if (!released)
                return false;

            freeItems.Push(item);

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