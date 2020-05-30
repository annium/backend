using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Pooling.Tests
{
    public class ObjectPoolTests
    {
        private const int Capacity = 5;
        private const int Jobs = 20;
        private const string Created = "Created";
        private const string Action = "ExecuteAction";
        private const string Disposed = "Disposed";

        [Fact]
        public void ObjectPool_Eager_FIFO_Works()
        {
            // arrange
            var (pool, logs) = CreatePool(PoolLoadMode.Eager, PoolStorageMode.Fifo);

            // act
            Run(pool);

            // assert
            // as eager - all workers Created
            logs.Where(x => x.Contains(Created)).Has(Capacity);
            // all job is done
            logs.Where(x => x.Contains(Action)).Has(Jobs);
            Enumerable.Range(0, Jobs).All(i => logs.Any(e => e.Contains($"{Action} {i}"))).IsTrue();
            // all workers are Disposed
            logs.Where(x => x.Contains(Disposed)).Has(Capacity);
        }

        [Fact]
        public void ObjectPool_Lazy_LIFO_Works()
        {
            // arrange
            var (pool, logs) = CreatePool(PoolLoadMode.Lazy, PoolStorageMode.Lifo);

            // act
            Run(pool);

            // assert
            // all job is done
            Enumerable.Range(0, Jobs).All(i => logs.Any(e => e.Contains($"{Action} {i}"))).IsTrue();
            // all workers are Disposed
            (logs.Where(x => x.Contains(Disposed)).Count() == logs.Where(x => x.Contains(Created)).Count()).IsTrue();
        }

        private (IObjectPool<Item>, IReadOnlyCollection<string>) CreatePool(
            PoolLoadMode loadMode,
            PoolStorageMode storageMode
        )
        {
            var nextId = 1u;
            var logs = new List<string>();

            void Log(string message)
            {
                lock (logs) logs.Add(message);
            }

            var pool = new ObjectPool<Item>(() => new Item(nextId++, Log), 5, loadMode, storageMode);

            return (pool, logs);
        }

        private void Run(
            IObjectPool<Item> pool
        )
        {
            var tasks = Enumerable.Range(0, Jobs).Select(i => Task.Run(() =>
            {
                var item = pool.Get();
                item.ExecuteAction(i);
                pool.Return(item);
            })).ToArray();
            Task.WaitAll(tasks);
            pool.Dispose();
        }

        private class Item : IDisposable
        {
            private readonly uint id;
            private readonly Action<string> log;

            public Item(
                uint id,
                Action<string> log
            )
            {
                this.id = id;
                this.log = log;
                log($"{id} {Created}");
            }

            public void ExecuteAction(int i)
            {
                Thread.Sleep(10);
                log($"{id} {ObjectPoolTests.Action} {i}");
            }

            public void Dispose()
            {
                log($"{id} {Disposed}");
            }
        }
    }
}