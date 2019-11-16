using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;

namespace Annium.Extensions.Pooling.Tests
{
    public class ObjectPoolTests
    {
        private const int capacity = 5;
        private const int jobs = 20;
        private const string created = "created";
        private const string action = "action";
        private const string disposed = "disposed";

        [Fact]
        public void ObjectPool_Eager_FIFO_Works()
        {
            // arrange
            var (pool, logs) = CreatePool(PoolLoadMode.Eager, PoolStorageMode.FIFO);

            // act
            Run(pool);

            // assert
            // as eager - all workers created
            logs.Where(x => x.Contains(created)).Has(capacity);
            // all job is done
            logs.Where(x => x.Contains(action)).Has(jobs);
            Enumerable.Range(0, jobs).All(i => logs.Any(e => e.Contains($"{action} {i}"))).IsTrue();
            // all workers are disposed
            logs.Where(x => x.Contains(disposed)).Has(capacity);
        }

        [Fact]
        public void ObjectPool_Lazy_LIFO_Works()
        {
            // arrange
            var (pool, logs) = CreatePool(PoolLoadMode.Lazy, PoolStorageMode.LIFO);

            // act
            Run(pool);

            // assert
            // all job is done
            Enumerable.Range(0, jobs).All(i => logs.Any(e => e.Contains($"{action} {i}"))).IsTrue();
            // all workers are disposed
            (logs.Where(x => x.Contains(disposed)).Count() == logs.Where(x => x.Contains(created)).Count()).IsTrue();
        }

        private (IObjectPool<Item>, IReadOnlyCollection<string>) CreatePool(
            PoolLoadMode loadMode,
            PoolStorageMode storageMode
        )
        {
            var nextId = 1u;
            var logs = new List<string>();
            void log(string message)
            {
                lock (logs) logs.Add(message);
            }
            var pool = new ObjectPool<Item>(() => new Item(nextId++, log), 5, loadMode, storageMode);

            return (pool, logs);
        }

        private void Run(
            IObjectPool<Item> pool
        )
        {
            var tasks = Enumerable.Range(0, jobs).Select(i => Task.Run(() =>
            {
                var item = pool.Get();
                item.Action(i);
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
                log($"{id} {created}");
            }

            public void Action(int i)
            {
                Thread.Sleep(10);
                log($"{id} {action} {i}");
            }

            public void Dispose()
            {
                log($"{id} {disposed}");
            }
        }
    }
}