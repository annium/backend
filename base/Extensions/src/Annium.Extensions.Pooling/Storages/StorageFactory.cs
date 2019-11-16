using System;
namespace Annium.Extensions.Pooling.Storages
{
    internal static class StorageFactory
    {
        public static IStorage<T> Create<T>(PoolStorageMode mode, int capacity) => mode switch
        {
            PoolStorageMode.FIFO => new QueueStorage<T>(capacity),
            PoolStorageMode.LIFO => new StackStorage<T>(capacity),
            _ => throw new NotImplementedException($"Unsupported storage mode {mode}"),
        };
    }
}