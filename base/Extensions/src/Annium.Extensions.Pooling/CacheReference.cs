using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Pooling
{
    public class CacheReference<TValue> : IAsyncDisposable
        where TValue : notnull
    {
        public TValue Value { get; private set; }
        private readonly Func<Task> dispose;

        public CacheReference(
            TValue value,
            Func<Task> dispose
        )
        {
            Value = value;
            this.dispose = dispose;
        }

        public async ValueTask DisposeAsync()
        {
            Value = default!;
            await dispose();
        }
    }
}